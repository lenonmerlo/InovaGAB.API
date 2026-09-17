using InovaGAB.API.Models;

namespace InovaGAB.API.Tests.UnitTests;

// calculo puro, sem banco: cobre ROI individual e consolidado (mesma formula)
public class RoiCalculatorTests
{
    [Fact]
    public void Calculate_ComInvestimentoERetorno_CalculaPercentualCorreto()
    {
        var roi = RoiCalculator.Calculate(25000, 75000);

        Assert.Equal(200, roi);
    }

    [Fact]
    public void Calculate_SemRetornoFinanceiro_RetornaZero()
    {
        var roi = RoiCalculator.Calculate(10000, 0);

        Assert.Equal(0, roi);
    }

    [Fact]
    public void Calculate_SemInvestimento_RetornaZero()
    {
        var roi = RoiCalculator.Calculate(0, 5000);

        Assert.Equal(0, roi);
    }

    [Fact]
    public void Calculate_InvestimentoIgualAoRetorno_RetornaZeroPorCento()
    {
        var roi = RoiCalculator.Calculate(50000, 50000);

        Assert.Equal(0, roi);
    }

    [Fact]
    public void Calculate_Consolidado_SomaProjetosAntesDeCalcular()
    {
        // simula o ROI consolidado do dashboard: soma investimento e
        // retorno de vários projetos antes de aplicar a mesma formula
        var totalInvestment = 25000m + 18000m;
        var totalReturn = 75000m + 420000m;

        var roi = RoiCalculator.Calculate(totalInvestment, totalReturn);

        var expected = (totalReturn - totalInvestment) / totalInvestment * 100;
        Assert.Equal(expected, roi);
    }
}
