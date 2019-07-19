using CRUDService.Model;
using FluentValidation;
using System.Linq;

namespace CRUDService.Validators
{
    internal class DefaultValidator : AbstractValidator<Entity>
    {
        public DefaultValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
