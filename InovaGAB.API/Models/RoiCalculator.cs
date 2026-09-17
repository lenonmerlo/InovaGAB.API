namespace InovaGAB.API.Models;

// formula de ROI usada tanto no calculo individual (Project.Roi) quanto
// no consolidado (DashboardService), para nao duplicar a regra
public static class RoiCalculator
{
    // 0 quando nao ha investimento ou retorno financeiro ainda registrado,
    // em vez de -100% (ver README, secao "Regras de negocio: Projeto")
    public static decimal Calculate(decimal investment, decimal financialReturn)
    {
        if (investment <= 0 || financialReturn <= 0)
        {
            return 0;
        }

        return (financialReturn - investment) / investment * 100;
    }
}
