using System;
using System.Threading.Tasks;
using DataAPI.DataStructures.PostBodies;
using DataAPI.DataStructures.Views;
using DataAPI.Service.DataStorage;
using DataAPI.Service.Objects;
using MongoDB.Driver;

namespace DataAPI.Service
{
    public class ViewManager
    {
        private readonly RdDataMongoClient rdDataClient;
        private readonly IMongoCollection<View> viewCollection;

        public ViewManager(RdDataMongoClient rdDataClient)
        {
            this.rdDataClient = rdDataClient;
            viewCollection = rdDataClient.BackendDatabase.GetCollection<View>(nameof(View));
        }

        public async Task<ViewInformation> CreateViewAsync(CreateViewBody body, string submitter)
        {
			var id = body.ViewId;
			if(string.IsNullOrEmpty(body.ViewId))
				id = GenerateViewId();
            if (await ExistsAsync(id))
                throw new DocumentAlreadyExistsException("ViewID already exists");
			
			
            var view = new View(id, body.Query, submitter, body.Expires);
            await viewCollection.InsertOneAsync(view);

            return new ViewInformation(id);
        }

        public async Task<bool> ExistsAsync(string viewId)
        {
            var view = await viewCollection.Find(x => x.Id == viewId).FirstOrDefaultAsync();
            if (view == null)
                return false;
            if (DateTime.UtcNow > view.Expires)
            {
                await DeleteViewAsync(viewId);
                return false;
            }
            return true;
        }

        public Task<View> GetView(string viewId)
        {
            return viewCollection.Find(x => x.Id == viewId).FirstOrDefaultAsync();
        }

        public async Task<string> GetViewQueryAsync(string viewId)
        {
            var view = viewCollection.Find(x => x.Id == viewId).FirstOrDefault();
            if (view == null)
                return null;
            if (DateTime.UtcNow > view.Expires)
            {
                await DeleteViewAsync(viewId);
                return null;
            }
            return view.Query;
        }

        public async Task<bool> DeleteViewAsync(string viewId)
        {
            var deleteResult = await viewCollection.DeleteOneAsync(x => x.Id == viewId);
            return deleteResult.IsAcknowledged && deleteResult.DeletedCount == 1;
        }

        private string GenerateViewId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
