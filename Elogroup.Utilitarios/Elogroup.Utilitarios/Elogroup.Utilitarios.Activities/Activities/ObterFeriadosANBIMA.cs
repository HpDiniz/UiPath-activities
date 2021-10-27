using System;
using System.Net;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using Elogroup.Utilitarios.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;

namespace Elogroup.Utilitarios.Activities
{
    [LocalizedDisplayName(nameof(Resources.ObterFeriadosANBIMA_DisplayName))]
    [LocalizedDescription(nameof(Resources.ObterFeriadosANBIMA_Description))]
    public class ObterFeriadosANBIMA : ContinuableAsyncCodeActivity
    {
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedDisplayName(nameof(Resources.ObterFeriadosANBIMA_Ano_DisplayName))]
        [LocalizedDescription(nameof(Resources.ObterFeriadosANBIMA_Ano_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<int> Ano { get; set; }

        [LocalizedDisplayName(nameof(Resources.ObterFeriadosANBIMA_Result_DisplayName))]
        [LocalizedDescription(nameof(Resources.ObterFeriadosANBIMA_Result_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<DataTable> Result { get; set; }

        #endregion


        #region Constructors

        public ObterFeriadosANBIMA()
        {
        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (Ano == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(Ano)));

            base.CacheMetadata(metadata);
        }

        public string GetResponse(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            // Inputs
            var ano = Ano.Get(context);


            string url = "https://www.anbima.com.br/feriados/fer_nacionais/" + ano.ToString() + ".asp";

            string htmlCode = GetResponse(url);

            string[] result = new Regex("Feriados nacionais para o ano de").Split(htmlCode);
            htmlCode = result[1];

            result = new Regex(@"[0-9]{1,2}\/[0-9]{1,2}\/" + ano.ToString().Substring(2, 2).ToString()).Split(htmlCode);

            foreach (string item in result)
                htmlCode = htmlCode.Replace(item.ToString(), "{#}");

            result = new Regex("{#}").Split(htmlCode);

            DataTable DTblFeriados = new DataTable();
            DTblFeriados.Columns.Add("Data", typeof(String));

            foreach (string item in result)
            {
                if (item.Contains("/"))
                {
                    DataRow newRow = DTblFeriados.NewRow();

                    string[] dateParser = item.ToString().Split(new char[] { '/' });
                    newRow["Data"] = new DateTime(Convert.ToInt32(ano), Convert.ToInt32(dateParser[1]), Convert.ToInt32(dateParser[0])).ToString("dd/MM/yyyy");

                    DTblFeriados.Rows.Add(newRow);
                }
            }

            // Outputs
            return (ctx) => {
                Result.Set(ctx, DTblFeriados);
            };
        }

        #endregion
    }
}

