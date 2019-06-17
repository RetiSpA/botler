using Botler.Dialogs.Scenari;
using Botler.Models;
using Microsoft.Bot.Builder;

using static Botler.Dialogs.Utility.Scenari;
namespace Botler.Builders
{
    /// <summary>
    /// Return a Scenario instance 
    /// </summary>
    public class ScenarioFactory
    {
        public static IScenario FactoryMethod(BotlerAccessors accessors, ITurnContext turn, string scenario, Intent intent)
        {
            IScenario new_scenario = null;

            if (scenario.Equals(Parking))
            {
                new_scenario = new ParkingScenario(accessors, turn);
            }

            if (scenario.Equals(Default))
            {
                new_scenario = new DefaultScenario(accessors, turn);
            }

            if (scenario.Equals(Autenticazione))
            {
                new_scenario = new AutenticazioneScenario(accessors, turn);
            }

            if (scenario.Equals(OutlookDescription))
            {
                new_scenario = new OutlookDescriptionScenario(accessors, turn);
            }

            if (scenario.Equals(Outlook))
            {
                new_scenario = new OutlookScenario(accessors, turn);
            }

            if (scenario.Equals(Supporto))
            {
                new_scenario = new SupportoScenario(accessors, turn);
            }

            if (scenario.Equals(SupportoDescription))
            {
                new_scenario = new SupportoDescriptionScenario();
            }

            if (new_scenario is null)
            {
                new_scenario = new DefaultScenario(accessors, turn);
            }
             // Never return a null Scenario.
            new_scenario.ScenarioIntent = intent;
            return new_scenario;
        }
    }
}