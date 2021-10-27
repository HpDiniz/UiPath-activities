using System.Activities.Presentation.Metadata;
using System.ComponentModel;
using System.ComponentModel.Design;
using Elogroup.BigQuery.Activities.Design.Designers;
using Elogroup.BigQuery.Activities.Design.Properties;

namespace Elogroup.BigQuery.Activities.Design
{
    public class DesignerMetadata : IRegisterMetadata
    {
        public void Register()
        {
            var builder = new AttributeTableBuilder();
            builder.ValidateTable();

            var categoryAttribute = new CategoryAttribute($"{Resources.Category}");

            builder.AddCustomAttributes(typeof(BigQueryScope), categoryAttribute);
            builder.AddCustomAttributes(typeof(BigQueryScope), new DesignerAttribute(typeof(BigQueryScopeDesigner)));
            builder.AddCustomAttributes(typeof(BigQueryScope), new HelpKeywordAttribute(""));

            builder.AddCustomAttributes(typeof(ExecuteQuery), categoryAttribute);
            builder.AddCustomAttributes(typeof(ExecuteQuery), new DesignerAttribute(typeof(ExecuteQueryDesigner)));
            builder.AddCustomAttributes(typeof(ExecuteQuery), new HelpKeywordAttribute(""));


            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
    }
}
