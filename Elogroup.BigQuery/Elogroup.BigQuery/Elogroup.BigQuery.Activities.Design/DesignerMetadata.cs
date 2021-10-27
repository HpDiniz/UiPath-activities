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

            builder.AddCustomAttributes(typeof(RealizarConsulta), categoryAttribute);
            builder.AddCustomAttributes(typeof(RealizarConsulta), new DesignerAttribute(typeof(RealizarConsultaDesigner)));
            builder.AddCustomAttributes(typeof(RealizarConsulta), new HelpKeywordAttribute(""));


            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
    }
}
