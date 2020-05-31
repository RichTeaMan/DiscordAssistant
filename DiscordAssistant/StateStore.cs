using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordAssistant
{
    public class StateStore
    {
        private readonly string StateStoreKey = "state";

        private readonly DataStore dataStore;

        public StateStore(DataStore dataStore)
        {
            this.dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
        }

        public async Task<State> FetchState()
        {
            State state;
            var stateLoadResponse = await dataStore.Load(StateStoreKey);
            if (stateLoadResponse.IsSuccess)
            {
                state = JsonConvert.DeserializeObject<State>(stateLoadResponse.Content);
            }
            else
            {
                state = State.CreateDefault();
            }
            return state;
        }

        public async Task SaveState(State state)
        {
            await dataStore.Save(StateStoreKey, state);
        }
    }
}
