## Manuella tester – Holiday Maker

För att kontrollera att Holiday Maker fungerar har följande testfall tagits fram. Testerna genomförs manuellt genom att använda programmets menyer och kontrollera resultatet.

| Test | Hur det kontrolleras | Förväntat resultat |
|---|---|---|
| Lediga och upptagna rum | Sök på datum där ett rum är bokat och ett annat är ledigt. | Bara det lediga rummet visas. |
| Incheckning samma dag | Skapa en bokning med incheckning samma dag som en tidigare bokning har utcheckning. | Bokningen godkänns. |
| Överlappande bokningar | Försök boka ett upptaget rum, även med datum som helt omsluter en befintlig bokning. | Bokningen nekas. |
| För många gäster | Försök boka ett rum för två personer med tre gäster. | Bokningen nekas. |
| Sökfilter och sortering | Kombinera ort, datum, pool, restaurang och avstånd. Testa sortering efter pris och betyg. | Rätt rum visas i rätt ordning. |
| Ändra antal gäster | Försök ändra en bokning till fler gäster än rummet tillåter. | Ändringen nekas. |
| Avbokning | Avboka ett rum och sök på samma datum igen. | Rummet blir tillgängligt. |
| Sparad information | Skapa en bokning, avsluta programmet och starta det igen. | Bokningen finns fortfarande kvar. |

**Testresultat:** Fyll i vilka tester som har genomförts och om de fungerade som förväntat.