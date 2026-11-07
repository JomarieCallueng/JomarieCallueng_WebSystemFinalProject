namespace GradingSystem.Services;

public static class GradeRemarks
{
    public static string For(int grade) => grade switch
    {
        >= 95 => "CMS",
        >= 90 => "SP",
        >= 85 => "SAP",
        >= 80 => "GP",
        >= 75 => "EP",
        _ => "BEP/FAILED"
    };
}