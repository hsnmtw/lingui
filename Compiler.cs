using System.Text;

namespace lingui;

public class Compiler {
    public void Compile(string fileName) {
        var lexer = new Lexer(fileName);
        var parser = new Parser(lexer);
        var module = parser.Parse();
        var assembly = $"{fileName.Split('.')[0]}.asm";
        using var stream = File.Open(assembly, FileMode.Create, FileAccess.Write);
        using var sw = new StreamWriter(stream);
        var sb = new StringBuilder();
        var db = 0;
        var map = new Dictionary<string,int>();

        sw.WriteLine(
            """
            format ELF64 executable 3
            use64
            segment readable executable
            SYS_READ  = 0
            SYS_WRITE = 1
            SYS_EXIT  = 60

            FD_STDIN  = 0
            FD_STDOUT = 1
            FD_STDERR = 2
            macro print str,len {
                mov rax,SYS_WRITE
                mov rdi,FD_STDOUT
                mov rsi,str
                mov rdx,len
                syscall
            }
            macro input str,len {
                mov rax,SYS_READ
                mov rdi,FD_STDIN
                mov rsi,str
                mov rdx,len
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
            entry main
            """
        );

        foreach(var constant in module.Constants) {
            sb.AppendLine($"db{db,-6}  : db \"{constant.Value.Trim('"')}\"");
            sb.AppendLine($"db{db}_len=$-{db}");
            map[constant.Name] = db++;
        }
        foreach(var variable in module.Variables) {
            sb.AppendLine($"db{db,-6}  : db \"{variable.Value.Trim('"')}\"");
            sb.AppendLine($"db{db}_len=$-db{db}");
            map[variable.Name] = db++;
        }
        foreach(var function in module.Functions) {
            sw.WriteLine($"{function.Name}:");
            
            foreach(var statement in function.Block.Statements) {
                if (statement is IDeclaration declaration) {
                    sb.AppendLine($"db{db}    : db \"{declaration.Value.Trim('"')}\"");
                    sb.AppendLine($"db{db}_len=$-db{db}");
                    map[declaration.Name] = db++;
                } 
                else if (statement is Print print) {
                    foreach(var value in print.Values) {
                        if (map.TryGetValue(value, out var ix)) {
                            //sw.WriteLine($"   mov rsi,db{ix}");
                            //sw.WriteLine($"   itoa rsi,db{ix},db{ix}_len");
                            sw.WriteLine($"   print db{ix},db{ix}_len");
                        } else {
                            sw.WriteLine($"   print db{db},db{db}_len");
                            sb.AppendLine($"db{db}    : db \"{value.Trim('"')}\"");
                            sb.AppendLine($"db{db}_len=$-db{db}");
                            db++;
                        }
                    }
                    sw.WriteLine("   call print_newline");
                }
                else if (statement is Input input) {
                    if (map.TryGetValue(input.Variable, out var ix)) {
                        sb.AppendLine($"db_{ix}:db \"{input.Message.Trim('"')}\"");
                        sb.AppendLine($"db_{ix}_len=$-db_{ix}");
                        sw.WriteLine($"   print db_{ix},db_{ix}_len");
                        sw.WriteLine($"   input db{ix},db{ix}_len");
                    }
                }
            }
        }

        sw.WriteLine(
            """
               exit 0
               ;;;;;;
            print_newline:
               print newline,1
               ret
            segment readable writable
            newline   db 10
            """);
        sw.WriteLine(sb);

        sw.Flush();
        sw.Close();

        Logger.Info($"Written to {assembly}");
    }
}