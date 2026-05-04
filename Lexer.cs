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
    NEW_LINE,
    CONST
}

public record Token (int Row, int Column,string Content, TokenType Type);

public record class Lexer (string FileName) {
    private readonly string sourceCode = File.ReadAllText(FileName);

    public int Position { get; private set; }
    public int Column { get; private set; }
    public int Row { get; private set; }

    public int Length => sourceCode.Length;

    public void SetPosition(int position) {
        if (position<0 || position>=Length) return;
        Position = position;
    }

    private const char NEW_LINE = '\n';

    public Token? Next() {
        
        Logger.AssertIsNotNull(sourceCode, $"[{FileName}] Source code cannot be null");

        if (Position >= sourceCode.Length) return null;        
        
        // skip white spaces and update column, row, position
        for (int i=Position;i<sourceCode.Length;++i) {
            Column++;
            Position = i;
            char c = sourceCode[i];
            if (c == NEW_LINE) {
                Column = 0;
                Row++;
                Position++;
                return new Token(Row, Column, "\\n", TokenType.NEW_LINE);
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
                if (sourceCode[i] != '"') {
                    Logger.Error($"unterminated string at {FileName}:{Row}", fail: true);
                }
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

        var type = TokenType.IDENTIFIER;
        
        if (content == "fn")      type = TokenType.FN       ;
        if (content == "program") type = TokenType.PROGRAM  ;
        if (IsString(content))    type = TokenType.STRING   ;
        if (IsNumber(content))    type = TokenType.NUMBER   ;
        if (content == "var")     type = TokenType.VAR      ;
        if (content == "const")   type = TokenType.CONST    ;
        if (content == ",")       type = TokenType.COMMA    ;
        if (content == "print")   type = TokenType.PRINT    ;
        if (content == "input")   type = TokenType.INPUT    ;
        if (content == "*")       type = TokenType.MULTIPLY ;
        if (content == "+")       type = TokenType.PLUS     ;
        if (content == "-")       type = TokenType.MINUS    ;
        if (content == "/")       type = TokenType.DIVIDE   ;
        if (content == "=")       type = TokenType.EQUALS   ;
        if (content == "end")     type = TokenType.END      ;  
        
        return new Token(Row, _column, content, type);
    }

    public static bool IsString(string content) {
        return content.Length >= 2 
            && content[0] == '"'
            && content[^1] == '"'
            ;
    }

    public static bool IsNumber(string content) {
        //return (c >= '0' && c <= '9') || c == '.';
        return double.TryParse(content, out _);
    }
}