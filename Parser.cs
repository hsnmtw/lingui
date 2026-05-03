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
        return ParseProgram(lexer);
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
        return ParseFunctions(lexer);
    }

    public static string[] ParseFunctions(Lexer lexer) {
        Token? token = lexer.Next();
        if (token == null) {
            Logger.Error("reached end of lexer", fail:true);
            return [];
        }
        if (token.Type != TokenType.FN) {
            Logger.Error($"expected fn at {lexer.FileName}:{lexer.Row}:{lexer.Column}, but got {token.Type} : '{token.Content}'", fail:true);
        }
        token = lexer.Next();
        if (token == null) {
            Logger.Error("reached end of lexer", fail:true);
            return [];
        }
        if (token.Type != TokenType.IDENTIFIER) {
            Logger.Error($"expected identifier at {lexer.FileName}:{lexer.Row}:{lexer.Column}, but got {token.Type} : '{token.Content}'", fail:true);
        }

        Dictionary<string,string> map = [];

        while ((token = lexer.Next()) != null && token.Type != TokenType.END) {
            switch(token.Type) {
                case TokenType.VAR: {
                    token = lexer.Next();
                    if (token == null) {
                        Logger.Error("reached end of lexer", fail:true);
                        return [];
                    }
                    if (token.Type != TokenType.IDENTIFIER) {
                        Logger.Error($"expected identifier at {lexer.FileName}:{lexer.Row}:{lexer.Column}, but got {token.Type} : '{token.Content}'", fail:true);
                    }
                    if (map.TryGetValue(token.Content, out _)) {
                        Logger.Error($"this variable identifier at {lexer.FileName}:{lexer.Row}:{lexer.Column}, was already defined previously : '{token.Content}'", fail:true);
                    }
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

        return [];
        
    }
}