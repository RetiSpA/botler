using Microsoft.Bot.Builder;
using static Botler.Dialogs.Utility.Scenari;
namespace Botler.Dialogs.Scenari
{
    public class ScenarioFactory
    {
        public static IScenario FactoryMethod(BotlerAccessors accessors, ITurnContext turn, string scenario)
        {
            if(scenario.Equals(Parking)) return new ParkingScenario(accessors, turn);

            if(scenario.Equals(Default)) return new DefaultScenario(accessors, turn);

            if(scenario.Equals(Autenticazione)) return new AutenticazioneScenario(accessors, turn);

            return new DefaultScenario(accessors, turn); // Never return a null Scenario.
        }
    }
}