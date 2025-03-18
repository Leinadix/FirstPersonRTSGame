# First-Person RTS Prototyp

Ein Prototyp für ein First-Person RTS auf dem Wasser.

## Installation

1. Stelle sicher, dass Python 3.7+ installiert ist
2. Installiere die Abhängigkeiten mit `pip install -r requirements.txt`
3. Starte das Spiel mit `python main.py`

## Steuerung

- **WASD**: Schiffsbewegung
- **Maus**: Kamerasteuerung
- **1-3**: Zwischen verschiedenen Schiffen wechseln
- **E**: Mit Ressourcen/Gebäuden interagieren
- **Leertaste**: Gebäude-Menü öffnen/schließen
- **I**: Schiffsinventar anzeigen
- **B**: Gebäudeinteraktionsmenü öffnen (wenn in der Nähe eines Gebäudes)
- **ESC**: Spiel beenden

## Spielmechaniken

### Ressourcensystem

- Verschiedene Ressourcen wie Öl, Kobalt, Geld und Kraftstoff
- Ressourcenlager auf der Karte
- Handel an Märkten
- Ressourcenbestand ist die Summe aller Schiffsinventare
- Ressourcen können nicht direkt gesammelt werden, sondern benötigen Gebäude zur Verarbeitung

### Gebäudesystem

- Verschiedene Gebäudetypen (Ölplattform, Raffinerie, Markt, Kobaltanreicherung)
- Gebäude-Menü über Leertaste
- Mit Gebäuden interagieren (E-Taste), um Ressourcen zu verarbeiten
- Gebäudeinventar anzeigen (B-Taste)
- Gebäude haben eigene Inventare, um Ressourcen zu speichern

### Schiffsystem

- Verschiedene Schiffstypen mit unterschiedlichen Eigenschaften
- Frachtsystem zum Transport von Ressourcen
- Kraftstoffverbrauch bei Bewegung
- Inventaranzeige über I-Taste
- Gesamtinventar ist die Summe aller Schiffsinventare

## Entwicklerhinweise

- Entwickelt mit Python und Pygame
- 3D-Darstellung durch Ray-Casting
- Wasseranimation durch Texturen
- Einfache Kollisionserkennung
- Minimap für Navigation

## Gameplay

- Sammle Ressourcen (Öl, Kobalt, etc.)
- Baue Gebäude zur Ressourcenverarbeitung
- Verbessere deine Flotte mit unterschiedlichen Schiffstypen
- Besiege den Gegner durch Zerstörung seiner Schiffe oder sammle die meisten Ressourcen vor Ablauf der Spielzeit

## Prototyp-Features

- Grundlegende Schiffssteuerung in First-Person-Perspektive
- Einfache Ressourcensammlung
- Rudimentäres Gebäudesystem
- Grundlegende Kampfmechanik 