namespace DomainRulesets.ConsoleApp.Exceptions;

public class ValidationException(string? message, Exception? innerException = null)
    : Exception(message, innerException);