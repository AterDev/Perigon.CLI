namespace Share.Constants;

public class RegexConst
{
    public const string SimplePasswordRegex = @"^(?!\d+$).{6,60}$";
    public const string NormalPasswordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,60}$";
    public const string StrongPasswordRegex =
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,60}$";
}
