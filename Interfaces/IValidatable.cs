namespace EduConnect.Interfaces
{
    public interface IValidatable
    {
        bool Validate(out string errorMessage);
    }
}