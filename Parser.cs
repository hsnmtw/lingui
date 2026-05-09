using System.Collections.Specialized;

namespace lingui;

public interface INode {}
public interface IInstruction : INode {}
public interface IDeclaration : IInstruction {
    string Name { get; set; }
    string Value { get; set; }
}
public record struct Operation(string LHS, char Operand, string RHS) : IInstruction;
public record struct Expression(string Constant, string Variable, Operation Operation) : IInstruction;
public record struct Assignment(string Variable, Expression Expression) : IInstruction;
public record struct Print(string Variable, string? Constant = default) : IInstruction;
public record struct Input(string Variable) : IInstruction;
public record struct Constant(string Name, string Value) : IDeclaration;
public record struct Variable(string Name, string Value) : IDeclaration;
public record struct Statement(IInstruction[] Instructions) : INode;
public record struct Block(Statement[] Statements) : INode;
public record struct Function(string Name, Constant[] Parameters, Block Block) : INode;
public record struct Module(
    Function[] Functions,
    Constant[] Constants,
    Variable[] Variables
) : INode;


public record class Parser (Lexer Lexer) {
    public Module Parse() {
        return new Module([],[],[]);
    }
}



public static class DirectParser {
    public static string[] Parse(Lexer lexer) {
        Expect(lexer, TokenType.MODULE);

        return [
            "format ELF64 executable",
            "use64",
            "entry main",
            """
            ; stdio.lingui

            SYS_READ  = 0
            SYS_WRITE = 1
            SYS_EXIT  = 60
            FD_STDIN  = 0
            FD_STDOUT = 1
            FD_STDERR = 2

            macro error str, len {
                mov rax, SYS_WRITE
                mov rdi, FD_STDERR
                mov rsi, str
                mov rdx, len
                syscall
            }
            macro print str, len {
                mov rax, SYS_WRITE
                mov rdi, FD_STDOUT
                mov rsi, str
                mov rdx, len
                syscall
            }
            macro input str, len {
                mov rax, SYS_READ
                mov rdi, FD_STDIN
                mov rsi, str
                mov rdx, len
                syscall
            } 
            macro exit code {
                mov rax, SYS_EXIT
                mov rdi, code
                syscall
            }
            ; -----------------------------------------------------------
            ; atoi: convert ASCII string at [buffer] into integer in rax
            ;   - reads until non-digit or null/newline
            ; -----------------------------------------------------------
            macro atoi result, _buffer {
                local .loop, .done
                    mov  rax, 0           ; same as (mov rax,0)
                    mov  rsi, _buffer
                .loop:
                    movzx rcx, byte [rsi]   ; gets the digit at position [rsi] from 
                    
                    ; if (rcx < '0' || rcx > '9') break;
                    cmp  rcx, '0'           ; make sure input is between 0 - 9                  
                    jl   .done              ; make sure input is between 0 - 9               
                    cmp  rcx, '9'           ; make sure input is between 0 - 9                  
                    jg   .done              ; make sure input is between 0 - 9            
                    ; rcx -= '0'
                    sub  rcx, '0'           ; ASCII digit → numeric value
                    ; rax *= 10
                    imul rax, rax, 10       ; result = result * 10
                    ; rax += rcx
                    add  rax, rcx           ; result += digit
                    ; rsi += 1
                    inc  rsi
                    jmp  .loop
                .done:
                    ; ret
                    mov result, rax
            }

            ; -----------------------------------------------------------
            ; itoa: convert integer in rax → ASCII in [out_buf]
            ;   - uses the stack to reverse digits
            ;   - writes length into [out_len]
            ; -----------------------------------------------------------
            macro itoa src,_out_buf, _out_len {
                local .extract, .push_loop, .pop_loop, .done
                    mov  rdi, _out_buf
                    mov  rcx, 0             ; digit counter
                    mov rax, src
                    test rax, rax
                    jnz  .extract
                    mov  byte [rdi], '0'    ; special-case: input was 0
                    mov  qword [_out_len], 1
                    ; ret
                    jmp .done
                .extract:
                    mov  rbx, 10
                .push_loop:
                    xor  rdx, rdx
                    div  rbx                ; rdx = rax % 10, rax = rax / 10
                    add  rdx, '0'
                    push rdx                ; push ASCII digit on stack
                    inc  rcx
                    test rax, rax
                    jnz  .push_loop
                    ; pop digits off stack into buffer (reverses them)
                    mov  rdi, _out_buf
                    mov  rbx, rcx           ; save count
                .pop_loop:
                    pop  rdx
                    mov  byte [rdi], dl
                    inc  rdi
                    loop .pop_loop
                    mov  qword [_out_len], rbx
                    ; ret
                .done:
                ; -----------------------------------------------------------
            }
            ; prints a single character
            macro printc chr {
                mov rax, chr      ; move the character on rax register
                push rax          ; push the character to the stack
                print rsp,1       ; syscall print the first character on the stack
                pop rax           ; restore the stack status
            }
            macro prints [chr*] {
                forward
                printc chr
            }
            """,
            ..ParseProgram(lexer),
            // "print newline,1",
            "exit 0",
            """
            ; -----------------------------------------------------------
            segment readable writeable
            buffer:      rb 32
            out_buf:     rb 32
            out_len:     dq 0
            newline:     db 10
            """
        ];
    }

    public static string[] ParseProgram(Lexer lexer) {
        Token? token = Expect(lexer, TokenType.IDENTIFIER);
        if (token is null) return [];
        return [
            $"; program {token.Content}",
            ..ParseFunctions(lexer),
        ];
    }

    public static string[] ParseFunctions(Lexer lexer) {
        Token? token = Expect(lexer, TokenType.FN);

        List<string> result = [];
        while (token != null && token.Type == TokenType.FN) {
            result.AddRange(ParseFunction(lexer));
            token = lexer.Next();
        }
        return [..result];
    }

    private static readonly string[] REGISTERS = [
        "R8",
        "R9",
        "R10",
        "R11",
        "R12",
        "R13",
        "R14",
        // "R15",
    ];

    public static string[] ParseFunction(Lexer lexer) {
        
        Token? token = Expect(lexer, TokenType.IDENTIFIER);

        Dictionary<string,string> var_map = [];
        int var_index = 0;
        List<string> result = [
            $"{token.Content}:"
        ];

        while ((token = lexer.Next()) != null && token.Type != TokenType.END) {
            switch(token.Type) {
                //case TokenType.CONST:
                case TokenType.VAR: {
                    token = Expect(lexer, TokenType.IDENTIFIER);

                    var id = token.Content;

                    Expect(lexer, TokenType.EQUALS);

                    token = Expect(lexer, TokenType.NUMBER);

                    if (var_map.ContainsKey(id)) {
                        Logger.Error($"this variable identifier at {lexer.FileName}:{lexer.Row}:{lexer.Column}, was already defined previously : '{token.Content}'", fail:true);
                    }
                    string register = var_map[id] = REGISTERS[var_index++];

                    result.Add($"mov {register}, {token.Content}");


                } break;
                case TokenType.IDENTIFIER: {
                    Logger.Error("NOT IMPLEMENTED !!");
                } break;
                case TokenType.PRINT: {
                    token = lexer.Next();
                    if (token is null) {
                        Logger.Error($"expected <constant> at {lexer.FileName}:{lexer.Row}:{lexer.Column}, but reached end", fail:true);
                        return [];
                    }
                    // identifier, number, string
                    if (token.Type == TokenType.IDENTIFIER) {
                        if (!var_map.TryGetValue(token.Content, out var register) || string.IsNullOrEmpty(register)) {
                            Logger.Error($"the identifier '{token.Content}' at {lexer.FileName}:{lexer.Row}:{lexer.Column}, was not decalred previously !", fail:true);
                            return [];
                        }
                        result.Add($"print {register},100");
                    }
                    else if (token.Type == TokenType.NUMBER) {
                        result.Add($"  mov r15,{token.Content}");
                        result.Add("  itoa r15,out_buf, out_len");
                        result.Add("  print out_buf, out_len");
                    }
                    else if (token.Type == TokenType.STRING) {
                        string c = string.Join(",", token.Content.Trim('"').Select(chr => chr switch {
                            '\r' => "10",
                            '\n' => "13",
                            var other => $"'{other}'"
                        }));
                        result.Add($"prints {c}");
                    }
                    else {
                        Logger.Error($"expected <identifier>|<number>|<string> at {lexer.FileName}:{lexer.Row}:{lexer.Column}, but got {token.Type}", fail:true);
                    }

                } break;
                case TokenType.INPUT: {
                    // we expect an identifier after input, the identifier is a defined variable in the scope
                    token = Expect(lexer, TokenType.IDENTIFIER);
                    if (!var_map.TryGetValue(token.Content, out var register) || string.IsNullOrEmpty(register)) {
                        Logger.Error($"the identifier '{token.Content}' at {lexer.FileName}:{lexer.Row}:{lexer.Column}, was not decalred previously !", fail:true);
                        return [];
                    }
                    result.Add($"input out_buf, out_len");
                    result.Add($"mov {register}, out_buf");
                } break;
                default : {
                    Logger.Error($"[123] unexpected token at {lexer.FileName}:{lexer.Row}:{lexer.Column}, got {token.Type} : '{token.Content}'", fail:true);
                    return [];
                }
            }
            Expect(lexer, TokenType.SEMI_COLON);

        }

        Expect(lexer, TokenType.FN);

        return [.. result];
    }

    private static Token Expect(Lexer lexer, TokenType type) {
        Token? token = lexer.Next(); // to ignore semi colon
        if (token is null) {
            Logger.Error($"expected '{type}' at {lexer.FileName}:{lexer.Row}:{lexer.Column}, but reached end", fail:true);
            throw new Exception();
        }

        if (token.Type != type) {
            Logger.Error($"expected '{type}' at {lexer.FileName}:{lexer.Row}:{lexer.Column}, but got {token.Type} : '{token.Content}'", fail:true);
        }
        return token;
    }
}