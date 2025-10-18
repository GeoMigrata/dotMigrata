using dotGeoMigrata.Core.Domain.Entities;

namespace dotGeoMigrata.Interfaces.Models;

internal interface IFeedbackModel
{
    void ApplyFeedback(World w, City c);
}