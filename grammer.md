BNF
=====

{program} :=

    program {identifer}
        {function}*

{function} :=
    fn {identifier}
        {statement}*
    end fn

{statement} :=
    {declaration}
    |
    {expression}
    |
    {instrction}

{declaration} :=
    var {identifier}[,{identifer}]*

{instruction} := 
    input {identifer}
    |
    print {string}|{identifier} [, {string}|{identifier}]*

{expression} :=
    {identifer} = [{number}|{identifer}] [+|-|*|/] [{number}|{identifer}]





