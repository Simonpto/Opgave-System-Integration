# 4. Domænemodel

`UserProfile` er den eneste entitet. Den bruger value objects, der validerer sig selv:

- **`Username`:** 3–30 tegn, kun bogstaver, tal, `_`, `.` og `-`. Skal være unikt.
- **`DisplayName`:** 1–50 tegn.
- **`Email`:** gyldigt format, gemmes med små bogstaver.

En profil er enten `Active` eller `Deactivated`, og en deaktiveret profil kan ikke ændres.
`MessageCount` tælles op, hver gang brugeren skriver en besked.

Ugyldige data giver en `DomainException`, som API'et svarer på med 400.
