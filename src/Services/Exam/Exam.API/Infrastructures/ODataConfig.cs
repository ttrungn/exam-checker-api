using Exam.Services.Models.Responses.Dashboard;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Exam.API.Infrastructures;

public static class ODataConfig
{
    public static IEdmModel GetEdmModel()
    {
        
        var builder = new ODataConventionModelBuilder();
        var dashboard = builder.EntitySet<DashboardSummaryResponse>("Dashboard");
        dashboard.EntityType.HasKey(x => x.ExamSubjectId);
        return builder.GetEdmModel();
    }
}
