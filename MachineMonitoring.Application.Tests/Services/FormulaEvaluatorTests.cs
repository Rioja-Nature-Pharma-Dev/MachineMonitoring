using MachineMonitoring.Application.Services;

namespace MachineMonitoring.Application.Tests.Services;

public class FormulaEvaluatorTests
{
    private readonly FormulaEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_SimpleArithmetic_ReturnsCorrectResult()
    {
        var vars = new Dictionary<string, decimal> { ["A"] = 10, ["B"] = 5 };
        var result = _evaluator.Evaluate("A + B", vars);
        Assert.Equal(15m, result);
    }

    [Fact]
    public void Evaluate_OEEFormula_CalculatesCorrectly()
    {
        var vars = new Dictionary<string, decimal>
        {
            ["AVAILABILITY"] = 95,
            ["PERFORMANCE"] = 90,
            ["QUALITY"] = 99
        };
        var result = _evaluator.Evaluate(
            "(AVAILABILITY * PERFORMANCE * QUALITY) / 10000",
            vars);

        Assert.Equal(84.645m, result);
    }

    [Fact]
    public void Evaluate_HealthIndexFormula_CalculatesCorrectly()
    {
        var vars = new Dictionary<string, decimal>
        {
            ["TEMP_MOTOR"] = 75,
            ["PRESSURE_HYDRAULIC"] = 200,
            ["SPEED_LINE"] = 120
        };

        var formula = "((150 - TEMP_MOTOR) / 150 * 100 + PRESSURE_HYDRAULIC / 250 * 100 + SPEED_LINE / 150 * 100) / 3";
        var result = _evaluator.Evaluate(formula, vars);

        // (50% + 80% + 80%) / 3 = 70%
        Assert.InRange(result, 69.9m, 70.1m);
    }

    [Fact]
    public void Evaluate_MissingVariable_ThrowsException()
    {
        var vars = new Dictionary<string, decimal> { ["A"] = 10 };
        Assert.Throws<FormulaEvaluationException>(() =>
            _evaluator.Evaluate("A + B", vars));
    }

    [Fact]
    public void Evaluate_DivisionByZero_ThrowsException()
    {
        var vars = new Dictionary<string, decimal> { ["A"] = 10, ["B"] = 0 };
        Assert.Throws<FormulaEvaluationException>(() =>
            _evaluator.Evaluate("A / B", vars));
    }

    [Fact]
    public void ExtractVariables_ReturnsAllVariables()
    {
        var formula = "(AVAILABILITY * PERFORMANCE * QUALITY) / 10000";
        var vars = _evaluator.ExtractVariables(formula);

        Assert.Contains("AVAILABILITY", vars);
        Assert.Contains("PERFORMANCE", vars);
        Assert.Contains("QUALITY", vars);
        Assert.Equal(3, vars.Count);
    }

    [Fact]
    public void TryValidate_ValidFormula_ReturnsTrue()
    {
        var availableVars = new[] { "A", "B", "C" };
        var isValid = _evaluator.TryValidate("(A + B) * C", availableVars, out var error);

        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void TryValidate_InvalidFormula_ReturnsFalseWithError()
    {
        var availableVars = new[] { "A" };
        var isValid = _evaluator.TryValidate("A + B", availableVars, out var error);

        Assert.False(isValid);
        Assert.NotNull(error);
    }

    [Fact]
    public void Evaluate_CaseInsensitiveVariables()
    {
        var vars = new Dictionary<string, decimal> { ["temp"] = 25 };
        var result = _evaluator.Evaluate("TEMP * 2", vars);
        Assert.Equal(50m, result);
    }
}
