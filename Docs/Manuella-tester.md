# Manuella tester – Holiday Maker

För att kontrollera att Holiday Maker fungerar har åtta manuella tester genomförts via programmets menyer. Syftet var att kontrollera att sökningar, bokningar och ändringar fungerar som de ska.

| Test | Hur det testades | Resultat |
|---|---|---|
| Lediga och upptagna rum | Sökte på datum där ett rum var bokat och ett annat ledigt. | Godkänt – endast lediga rum visades. |
| Incheckning samma dag | Bokade ett rum samma dag som en tidigare kund checkade ut. | Godkänt – bokningen gick igenom. |
| Överlappande bokningar | Försökte boka ett upptaget rum, även med datum som helt omslöt en befintlig bokning. | Godkänt – bokningarna nekades. |
| För många gäster | Försökte boka ett rum för två personer med tre gäster. | Godkänt – bokningen nekades. |
| Sökfilter och sortering | Kombinerade ort, datum, pool, restaurang och avstånd. Testade även sortering efter pris och betyg. | Godkänt – filtrering och sortering fungerade. |
| Ändra antal gäster | Försökte ändra en bokning till fler gäster än rummet tillät. | Godkänt – ändringen nekades. |
| Avbokning | Avbokade ett rum och sökte på samma datum igen. | Godkänt – rummet blev tillgängligt igen. |
| Sparad information | Skapade en bokning, avslutade programmet och startade om det. | Godkänt – bokningen fanns kvar. |

## Testresultat

Samtliga åtta manuella tester genomfördes och fungerade som förväntat.

Testerna visar att programmets viktigaste funktioner fungerar och att bokningsinformationen sparas i databasen även efter att programmet har startats om.