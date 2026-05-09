namespace lingui;

public static class Compiler {
    public static void Compile(string fileName) {
        var lexer = new Lexer(fileName);
        var parser = new Parser(lexer);
        var module = parser.Parse();
        
    }
}