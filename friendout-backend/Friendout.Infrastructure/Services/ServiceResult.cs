namespace Friendout.Infrastructure.Services;

/// <summary>
/// Représente le résultat d'une opération de service.
/// </summary>
/// <typeparam name="T">Le type des données retournées par l'opération.</typeparam>
public class ServiceResult<T>
{
    /// <summary>
    /// Obtient ou définit les données résultantes de l'opération.
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// Obtient ou définit une valeur indiquant si l'opération a réussi.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Obtient ou définit le message d'erreur en cas d'échec de l'opération.
    /// </summary>
    public string ErrorMessage { get; set; }
    
    /// <summary>
    /// Crée une instance de <see cref="ServiceResult{T}"/> pour indiquer le succès de l'opération.
    /// </summary>
    /// <param name="data">Les données résultantes de l'opération.</param>
    /// <returns>Une nouvelle instance de <see cref="ServiceResult{T}"/> indiquant le succès de l'opération.</returns>
    public static ServiceResult<T> Success(T data)
    {
        return new ServiceResult<T> { Data = data, IsSuccess = true };
    }

    /// <summary>
    /// Crée une instance de <see cref="ServiceResult{T}"/> pour indiquer l'échec de l'opération avec un message d'erreur spécifié.
    /// </summary>
    /// <param name="errorMessage">Le message d'erreur indiquant la raison de l'échec de l'opération.</param>
    /// <returns>Une nouvelle instance de <see cref="ServiceResult{T}"/> indiquant l'échec de l'opération.</returns>
    public static ServiceResult<T> Failure(string errorMessage)
    {
        return new ServiceResult<T> { IsSuccess = false, ErrorMessage = errorMessage };
    }
}