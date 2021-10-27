using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using System.Activities.Statements;
using System.ComponentModel;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;
using Elogroup.BigQuery.Activities.Properties;
using System.IO;
using Newtonsoft.Json.Linq;
using Google.Cloud.BigQuery.V2;
using Google.Apis.Auth.OAuth2;

namespace Elogroup.BigQuery.Activities
{
    [LocalizedDisplayName(nameof(Resources.BigQueryScope_DisplayName))]
    [LocalizedDescription(nameof(Resources.BigQueryScope_Description))]
    public class BigQueryScope : ContinuableAsyncNativeActivity
    {
        #region Properties

        [Browsable(false)]
        public ActivityAction<IObjectContainer​> Body { get; set; }

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedDisplayName(nameof(Resources.BigQueryScope_Credentials_DisplayName))]
        [LocalizedDescription(nameof(Resources.BigQueryScope_Credentials_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> Credentials { get; set; }

        // A tag used to identify the scope in the activity context
        internal static string ParentContainerPropertyTag => "ScopeActivity";

        // Object Container: Add strongly-typed objects here and they will be available in the scope's child activities.
        private readonly IObjectContainer _objectContainer;

        #endregion


        #region Constructors

        public BigQueryScope(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;

            Body = new ActivityAction<IObjectContainer>
            {
                Argument = new DelegateInArgument<IObjectContainer> (ParentContainerPropertyTag),
                Handler = new Sequence { DisplayName = Resources.Do }
            };
        }

        public BigQueryScope() : this(new ObjectContainer())
        {

        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (Credentials == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(Credentials)));

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<NativeActivityContext>> ExecuteAsync(NativeActivityContext  context, CancellationToken cancellationToken)
        {
            // Inputs
            var credentials = Credentials.Get(context);
            
            // --------- My Code ----------

            GoogleCredential googleCredentials = GoogleCredential.FromFile(credentials);

            JObject jsonData = JObject.Parse(File.ReadAllText(credentials));

            BigQueryClient client = BigQueryClient.Create(jsonData.GetValue("project_id").ToString(), googleCredentials);

            _objectContainer.Add<BigQueryClient>(client);

            // -----------------------------

            return (ctx) => {
                // Schedule child activities
                if (Body != null)
				    ctx.ScheduleAction<IObjectContainer>(Body, _objectContainer, OnCompleted, OnFaulted);

                // Outputs
            };
        }

        #endregion


        #region Events

        private void OnFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            faultContext.CancelChildren();
            Cleanup();
        }

        private void OnCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            Cleanup();
        }

        #endregion


        #region Helpers
        
        private void Cleanup()
        {
            var disposableObjects = _objectContainer.Where(o => o is IDisposable);
            foreach (var obj in disposableObjects)
            {
                if (obj is IDisposable dispObject)
                    dispObject.Dispose();
            }
            _objectContainer.Clear();
        }

        #endregion
    }
}

