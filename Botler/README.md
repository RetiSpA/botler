# Botler
Questo bot è stato creato usando:
- [Microsoft Bot Framework](https://dev.botframework.com)
- Usa [LUIS](https://luis.ai) il quale implementa la parte di AI.
- [QnA Maker](https://www.qnamaker.ai/) per la Knowledge base, utile a rispondere alle domande
- [Graph API](https://developer.microsoft.com/en-us/graph/) Per conettere il tuo profilo Microsoft al Bot, ed utilizzare tutte le sue funzionalità
- [CosmoDB API per MongoDB](https://docs.microsoft.com/en-us/azure/cosmos-db/mongodb-introduction) Usato per salvare dei file JSON che rappresentano lo stato del bot in un determinato turno di conversazine, usato per risalire al contesto della conversazione.

## Botler - Un Agente di Conversazione con Contesto -
Botler è in grado di riconoscere il contesto della converazione in cui si trova, da rendere più 'umana' e 'intelligente' la converazione e la sua interazione con gli utenti.
Non avrai bisogno di ripetere una frase o specificare ulteriori dettagli per continuare a comunicare con Botler.

Prendiamo questo esempio di un utente che vuole leggere le sue email in date diverse:

![alt text](https://github.com/RetiSpA/botler/blob/test-context/Docs/lettura-mail-esempio.png)

Il Bot sa che deve leggere le mail in quelle date, e l'utente non avrà bisogno di ripetersi più di una volta.


## Per provare questa repo di Botler
- Clona questa repository
```bash
git clone https://github.com/RetiSpA/botler/tree/test-context
```

[Optional] Puoi chiedere in privato il file '*.bot' con la configurazione dei servizi Microsoft usati.

### Testare il Bot usando Bot Framework Emulator
[Microsoft Bot Framework Emulator](https://aka.ms/botframework-emulator) E' una applicazione desktop utile per debuggare il bot, e testare le sue funzionalità, via tunnel o in locale.
- Installa il Bot Framework Emulatore [qui](https://aka.ms/botframework-emulator).
#### Connettiti al Bot Framework Emulator
- lancia il  Bot Framework Emulator
- File -> Apri il bot e naviga  `Botler\botler\' folder
- Seleziona `BasicBot.bot` file

# Autori
- Mattia Formenti
- Andrea Guzzo
- Giammarco Moscariello


# Contribuzioni

Leggi la Wiki per capire come contribuire e come usare al meglio il nostro Botler.
