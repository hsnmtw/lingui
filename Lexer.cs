namespace lingui;

public enum TokenType {
    FN,
    PROGRAM,
    STRING,
    NUMBER,
    VAR,
    COMMA,
    PRINT,
    INPUT,
    MULTIPLY,
    PLUS,
    MINUS,
    DIVIDE,
    EQUALS,
    END,
    IDENTIFIER,
}

public record Token (int Row, int Column,string Content, TokenType Type);

public record class Lexer (string fileName, string sourceCode) {
    public int Position { get; private set; }
    public int Column { get; private set; }
    public int Row { get; private set; }

    private const char NEW_LINE = '\n';

    public Token? Next() {
        
        Logger.AssertIsNotNull(sourceCode, $"[{fileName}] Source code cannot be null");

        if (Position >= sourceCode.Length) return null;        
        
        // skip white spaces and update column, row, position
        for (int i=Position;i<sourceCode.Length;++i) {
            Column++;
            Position = i;
            char c = sourceCode[i];
            if (c == NEW_LINE) {
                Column = 0;
                Row++;
            }
            if (!char.IsWhiteSpace(c)) {
                break;
            }
        }

        // get the next token, update column, row, position
        int _column = Column;
        int start = Position;
        for (int i=Position;i<sourceCode.Length;++i) {
            char c = sourceCode[i];
            if (c == '"') {
                //look for closing '"'
                while (++i < sourceCode.Length && sourceCode[i] != '"') Column++;
                Position=i;
                break;
            }
            if (char.IsWhiteSpace(c) || c == ',') {
                break;
            }
            Column++;
            Position = i;
        }

        string content = sourceCode[start..++Position];

        if (content.Trim().Length == 0) return null;
        
        if (content == "fn")      return new Token(Row, _column, content, TokenType.FN        );
        if (content == "program") return new Token(Row, _column, content, TokenType.PROGRAM   );
        if (IsString(content[0])) return new Token(Row, _column, content, TokenType.STRING    );
        if (IsNumber(content[0])) return new Token(Row, _column, content, TokenType.NUMBER    );
        if (content == "var")     return new Token(Row, _column, content, TokenType.VAR       );
        if (content == ",")       return new Token(Row, _column, content, TokenType.COMMA     );
        if (content == "print")   return new Token(Row, _column, content, TokenType.PRINT     );
        if (content == "input")   return new Token(Row, _column, content, TokenType.INPUT     );
        if (content == "*")       return new Token(Row, _column, content, TokenType.MULTIPLY  );
        if (content == "+")       return new Token(Row, _column, content, TokenType.PLUS      );
        if (content == "-")       return new Token(Row, _column, content, TokenType.MINUS     );
        if (content == "/")       return new Token(Row, _column, content, TokenType.DIVIDE    );
        if (content == "=")       return new Token(Row, _column, content, TokenType.EQUALS    );
        if (content == "end")     return new Token(Row, _column, content, TokenType.END       );    
                                  return new Token(Row, _column, content, TokenType.IDENTIFIER);
    }

    public static bool IsString(char c) {
        return c == '"';
    }

    public static bool IsNumber(char c) {
        return (c >= '0' && c <= '9') || c == '.';
    }
}