namespace JoyScript
{
    public enum OpCode : byte
    {
        NOP,
        Label,

        // stack
        Pop,
        PushValueLiteral,
        PushValue,
        DuplicateTop,

        // 
        LoadVariable, 
        StoreVariable,
        LoadRegister,
        StoreRegister,
        LoadTableKey,
        StoreTableKey,
        StoreTableKeyLiteral,
        LoadTableKeyLiteral,
        StoreTableKVLiteral,

        //
        CreateFunction,

        // ip
        Jump,
        JumpIf,
        Call,
        Return,

        // math
        Add,
        Sub,
        Neg,
        Mul,
        Div,
        Mod,
        Inc,
        Dec,

        // relation
        Equal,
        LowerThan,
        LowerEqualThan,
        GreaterThan,
        GreaterEqualThan,

    }
}