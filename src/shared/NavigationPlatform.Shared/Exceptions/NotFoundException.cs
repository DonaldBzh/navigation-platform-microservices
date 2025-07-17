using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.Shared.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"{name} ({key}) was not found.") { }

    public NotFoundException(string message)
        : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message = "You do not have access to this resource.")
        : base(message) { }
}

public class ApplicationValidationException : Exception
{
    public ApplicationValidationException(string message)
       : base(message) { }
}

public class GoneException : Exception
{
    public GoneException(string message)
       : base(message) { }
}

public class BadRequestException : Exception
{
    public BadRequestException(string message)
       : base(message) { }
}
