namespace lingui;

public static class Parser {
    public static string[] Parse(Lexer lexer) {
        Token? token = lexer.Next();
        if (token == null) {
            Logger.Error("reached end of lexer", fail:true);
            return [];
        }
        if (token.Type != TokenType.PROGRAM) {
            Logger.Error($"expected prgram at {lexer.FileName}:{lexer.Row}:{lexer.Column}, but got {token.Type} : '{token.Content}'", fail:true);
        }
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
            """,
            ..ParseProgram(lexer)
        ];
    }

    public static string[] ParseProgram(Lexer lexer) {
        Token? token = lexer.Next();
        if (token == null) {
            Logger.Error("reached end of lexer", fail:true);
            return [];
        }
        if (token.Type != TokenType.IDENTIFIER) {
            Logger.Error($"expected identidfier at {lexer.FileName}:{lexer.Row}:{lexer.Column}, but got {token.Type} : '{token.Content}'", fail:true);
        }
        return [
            $"; program {token.Content}",
            ..ParseFunctions(lexer)
        ];
    }

    public static string[] ParseFunctions(Lexer lexer) {
        Token? token = lexer.Next();
        if (token == null) {
            Logger.Error("[ParseFunctions] reached end of lexer", fail:true);
            return [];
        }

        System.Console.WriteLine("--------------------------");
        System.Console.WriteLine(token);
        System.Console.WriteLine("--------------------------");


        List<string> result = [];
        while (token != null && token.Type == TokenType.FN) {
            result.AddRange(ParseFunction(lexer));
            // int position = lexer.Position;
            token = lexer.Next();
            // lexer.SetPosition(position);
        }
        return [..result];
    }

    private static string[] REGISTERS = [
        "R8",
        "R9",
        "R10",
        "R11",
        "R12",
        "R13",
        "R14",
        "R15",
    ];

    public static string[] ParseFunction(Lexer lexer) {
        System.Console.WriteLine("/////////////////////////////");
        // Token? token = lexer.Next();
        // if (token == null) return [];
        // if (token.Type != TokenType.FN) {
        //     Logger.Error($"expected fn at {lexer.FileName}:{lexer.Row}:{lexer.Column}, but got {token.Type} : '{token.Content}'", fail:true);
        // }
        Token? token = lexer.Next();
        if (token == null) {
            Logger.Error("[ParseFunction/1] reached end of lexer", fail:true);
            return [];
        }
        if (token.Type != TokenType.IDENTIFIER) {
            Logger.Error($"expected identifier at {lexer.FileName}:{lexer.Row}:{lexer.Column}, but got {token.Type} : '{token.Content}'", fail:true);
        }

        Dictionary<string,string> var_map = [];
        int var_index = 0;
        List<string> result = [
            $"{token.Content}:"
        ];

        while ((token = lexer.Next()) != null && token.Type != TokenType.END) {
            switch(token.Type) {
                //case TokenType.CONST:
                case TokenType.VAR: {
                    token = lexer.Next();
                    if (token == null) {
                        Logger.Error("[ParseFunction/2] reached end of lexer", fail:true);
                        return [];
                    }
                    if (token.Type != TokenType.IDENTIFIER) {
                        Logger.Error($"expected identifier at {lexer.FileName}:{lexer.Row}:{lexer.Column}, but got {token.Type} : '{token.Content}'", fail:true);
                    }

                    var id = token.Content;

                    token = lexer.Next(); // must be Equals
                    if (token is null) {
                        Logger.Error($"expected '=' at {lexer.FileName}:{lexer.Row}:{lexer.Column}, but reached end", fail:true);
                        return [];
                    }

                    if (token.Type != TokenType.EQUALS) {
                        Logger.Error($"expected '=' at {lexer.FileName}:{lexer.Row}:{lexer.Column}, but got {token.Type} : '{token.Content}'", fail:true);
                    }

                    token = lexer.Next();

                    if (token is null) {
                        Logger.Error($"expected value at {lexer.FileName}:{lexer.Row}:{lexer.Column}, but reached end", fail:true);
                        return [];
                    }

                    if (token.Type != TokenType.NUMBER) {
                        Logger.Error($"expected number at {lexer.FileName}:{lexer.Row}:{lexer.Column}, but got {token.Type} : '{token.Content}'", fail:true);
                    }


                    if (var_map.ContainsKey(id)) {
                        Logger.Error($"this variable identifier at {lexer.FileName}:{lexer.Row}:{lexer.Column}, was already defined previously : '{token.Content}'", fail:true);
                    }
                    string register = var_map[id] = REGISTERS[var_index++];

                    result.Add($"mov {register}, {token.Content}");

                } break;
                case TokenType.IDENTIFIER: break;
                case TokenType.PRINT: break;
                case TokenType.INPUT: break;
                default : {
                    Logger.Error($"unexpected token at {lexer.FileName}:{lexer.Row}:{lexer.Column}, got {token.Type} : '{token.Content}'", fail:true);
                    return [];
                }
            }
        }

        lexer.Next();

        return [.. result];
    }
}