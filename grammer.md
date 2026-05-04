BNF
=====
```
<program> ::=

    program <identifer>
        <function>*



<function> ::=

    fn <identifier>
        <statement>*
    end fn



<statement> ::=

    <declaration>
    |
    <expression>
    |
    <instrction>



<declaration> ::=

    var <identifier>[ = <value>] [,<identifer> [ = <value>]]*
    |
    const <identifier> = <value>



<instruction> ::= 

    input <identifer>
    |
    print <string>|<identifier> [, <string>|<identifier>]*



<expression> ::=

    <identifer> = <number>|<identifer> [[+|-|*|/] [<number>|<identifer>]]*
```