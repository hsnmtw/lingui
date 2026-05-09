namespace lingui;

public static class Program {
    static void Main(string[]args) {
        var fileName = args.Length > 0 ? args[0] : "?";
        if (fileName == "?" || !File.Exists(fileName)) {
            Console.WriteLine("Usage: dotnet run <file-name>");
            return;
        }
        
        // var lexer = new Lexer(FileName: args[0]);
        // var i=1;
        // for (var token = lexer.Next(); token != null; token = lexer.Next()) {
        //     System.Console.WriteLine("{0,3} {1,-15} [{2,2} / {3,2}] : {4}",i++, token.Type, token.Row, token.Column, token.Content);
        // }

        // foreach (var line in Parser.Parse(lexer)) {
        //     System.Console.WriteLine(line);
        // }

        // string s = "123";
        // for(int i=0;i<s.Length;++i) {
        //     System.Console.WriteLine((s[i] - '0') * 2);
        // }

        var compiler = new Compiler();
        compiler.Compile(fileName);
    }
}
