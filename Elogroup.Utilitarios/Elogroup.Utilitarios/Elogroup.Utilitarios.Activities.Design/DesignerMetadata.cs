using System.Activities.Presentation.Metadata;
using System.ComponentModel;
using System.ComponentModel.Design;
using Elogroup.Utilitarios.Activities.Design.Designers;
using Elogroup.Utilitarios.Activities.Design.Properties;

namespace Elogroup.Utilitarios.Activities.Design
{
    public class DesignerMetadata : IRegisterMetadata
    {
        public void Register()
        {
            var builder = new AttributeTableBuilder();
            builder.ValidateTable();

            var categoryAttribute = new CategoryAttribute($"{Resources.Category}");

            builder.AddCustomAttributes(typeof(ObterFeriadosANBIMA), categoryAttribute);
            builder.AddCustomAttributes(typeof(ObterFeriadosANBIMA), new DesignerAttribute(typeof(ObterFeriadosANBIMADesigner)));
            builder.AddCustomAttributes(typeof(ObterFeriadosANBIMA), new HelpKeywordAttribute(""));

            builder.AddCustomAttributes(typeof(ConverterDataTableParaHTML), categoryAttribute);
            builder.AddCustomAttributes(typeof(ConverterDataTableParaHTML), new DesignerAttribute(typeof(ConverterDataTableParaHTMLDesigner)));
            builder.AddCustomAttributes(typeof(ConverterDataTableParaHTML), new HelpKeywordAttribute(""));


            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
    }
}
