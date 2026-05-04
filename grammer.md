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

    var <identifier> = <value> 
    /// [,<identifer> [ = <value>]]*
    /// |
    /// const <identifier> = <value> [,<identifer> [ = <value>]]*

<value> ::=
    number

<instruction> ::= 

    input <identifer>
    |
    print <identifier>|<constant>
    ///[, <string>|<identifier>]*

<constant> ::=
    <number> 
    | 
    <string>

<expression> ::=

    <identifer> = <number>|<identifer> [[+|-|*|/] [<number>|<identifer>]]*
```