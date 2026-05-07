using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MachineMonitoring.Application.Services;

/// <summary>
/// Evalua formulas matematicas con variables nombradas.
/// Soporta: +, -, *, /, parentesis, funciones (IIF, ABS, MIN, MAX)
/// Ejemplo: "(AVAILABILITY * PERFORMANCE * QUALITY) / 10000"
/// </summary>
public sealed class FormulaEvaluator
{
    private static readonly Regex VariableRegex = new(@"\b([A-Z_][A-Z0-9_]*)\b", RegexOptions.Compiled);

    /// <summary>
    /// Evalua una formula con variables proporcionadas
    /// </summary>
    /// <param name="formula">Expresion matematica (ej: "(A + B) * 2")</param>
    /// <param name="variables">Diccionario de variables (case-insensitive)</param>
    /// <returns>Resultado numerico</returns>
    /// <exception cref="FormulaEvaluationException">Si la formula es invalida</exception>
    public decimal Evaluate(string formula, IReadOnlyDictionary<string, decimal> variables)
    {
        if (string.IsNullOrWhiteSpace(formula))
            throw new FormulaEvaluationException("Formula cannot be empty.");

        var normalizedVars = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in variables)
            normalizedVars[key] = value;

        // Replace variables with their values
        var expression = VariableRegex.Replace(formula, match =>
        {
            var varName = match.Groups[1].Value;

            // Allow built-in functions to pass through
            if (IsBuiltInFunction(varName))
                return varName;

            if (!normalizedVars.TryGetValue(varName, out var value))
                throw new FormulaEvaluationException($"Variable '{varName}' not provided.");

            return value.ToString(CultureInfo.InvariantCulture);
        });

        try
        {
            using var dataTable = new DataTable();
            var result = dataTable.Compute(expression, string.Empty);

            return result switch
            {
                decimal d => d,
                double dbl => (decimal)dbl,
                int i => i,
                long l => l,
                _ => Convert.ToDecimal(result, CultureInfo.InvariantCulture)
            };
        }
        catch (Exception ex)
        {
            throw new FormulaEvaluationException(
                $"Failed to evaluate formula '{formula}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Valida que una formula sea sintacticamente correcta
    /// </summary>
    public bool TryValidate(string formula, IReadOnlyCollection<string> availableVariables, out string? error)
    {
        error = null;

        try
        {
            var dummyVars = availableVariables.ToDictionary(v => v, v => 1m);
            Evaluate(formula, dummyVars);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Extrae los nombres de variables usadas en una formula
    /// </summary>
    public IReadOnlyList<string> ExtractVariables(string formula)
    {
        if (string.IsNullOrWhiteSpace(formula))
            return Array.Empty<string>();

        var variables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in VariableRegex.Matches(formula))
        {
            var name = match.Groups[1].Value;
            if (!IsBuiltInFunction(name))
                variables.Add(name);
        }

        return variables.ToList();
    }

    private static bool IsBuiltInFunction(string name) => name.ToUpperInvariant() switch
    {
        "IIF" or "ABS" or "MIN" or "MAX" or "SUM" or "AVG" or "TRUE" or "FALSE" => true,
        _ => false
    };
}

public sealed class FormulaEvaluationException : Exception
{
    public FormulaEvaluationException(string message) : base(message) { }
    public FormulaEvaluationException(string message, Exception inner) : base(message, inner) { }
}
