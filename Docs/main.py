import pygame
import sys
import numpy as np
from game.constants import *
from game.player import Player
from game.world import World
from game.ui import UI
from game.resource_manager import ResourceManager

class Game:
    def __init__(self):
        """Initialisiert das Spiel"""
        pygame.init()
        
        # Fenstererstellung
        self.screen = pygame.display.set_mode((SCREEN_WIDTH, SCREEN_HEIGHT))
        pygame.display.set_caption("First-Person RTS Prototyp")
        
        # Spielzustand
        self.game_state = "playing"  # "playing", "paused", "menu", etc.
        self.running = True  # Spiel läuft
        
        # Zeit
        self.clock = pygame.time.Clock()
        self.dt = 0  # Delta time (Zeitdifferenz) für gleichmäßige Bewegung
        
        # Welt erstellen
        self.world = World()
        
        # Spieler erstellen (in der Mitte der Karte)
        start_x = WORLD_SIZE / 2
        start_y = 1.8  # Augenhöhe
        start_z = WORLD_SIZE / 2
        self.player = Player(start_x, start_y, start_z, self.world)
        
        # Spielkomponenten
        self.resource_manager = ResourceManager()
        
        # UI erstellen
        self.ui = UI(self.screen, self.player, self.resource_manager)
        
        # Maussteuerung
        pygame.mouse.set_visible(False)
        pygame.event.set_grab(True)
        
    def handle_events(self):
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                self.running = False
            
            # Taste gedrückt
            elif event.type == pygame.KEYDOWN:
                if event.key == pygame.K_ESCAPE:
                    if self.game_state == "playing":
                        self.game_state = "paused"
                        pygame.mouse.set_visible(True)
                        pygame.event.set_grab(False)
                    else:
                        self.game_state = "playing"
                        pygame.mouse.set_visible(False)
                        pygame.event.set_grab(True)
                
                # Schiffswechsel mit Tasten 1-3
                if self.game_state == "playing":
                    if event.key == pygame.K_1:
                        self.player.switch_ship(0)
                    elif event.key == pygame.K_2:
                        self.player.switch_ship(1)
                    elif event.key == pygame.K_3:
                        self.player.switch_ship(2)
                    
                    # Gebäudemenü
                    elif event.key == pygame.K_SPACE:
                        self.ui.toggle_building_menu()
                    
                    # Interaktion
                    elif event.key == pygame.K_i:
                        self.ui.toggle_ship_inventory()
                    
                    # Gebäudeinteraktionsmenü
                    elif event.key == pygame.K_b:
                        # Finde das nächstgelegene Gebäude
                        nearest_building = None
                        min_distance = float('inf')
                        
                        for building in self.world.buildings:
                            distance = ((self.player.x - building.x) ** 2 + 
                                       (self.player.z - building.z) ** 2) ** 0.5
                            
                            if distance < min_distance and distance < 50:  # Max 50 Einheiten Entfernung
                                min_distance = distance
                                nearest_building = building
                        
                        if nearest_building:
                            self.ui.toggle_building_interaction(nearest_building)
                        else:
                            self.ui.add_feedback_message("Kein Gebäude in der Nähe", feedback_type="warning")
                    
                    # Interaktion
                    elif event.key == pygame.K_e:
                        interact_result = self.player.interact()
                        
                        # Feedback basierend auf Interaktionsergebnis
                        if self.player.nearby_objects:
                            obj = self.player.nearby_objects[0]
                            if interact_result:
                                if obj["type"] == "resource":
                                    # Dieser Fall sollte nie auftreten, da Ressourcen nicht direkt aufgesammelt werden können
                                    pass
                                elif obj["type"] == "building":
                                    building_type = obj["object"].type
                                    if building_type == "Markt":
                                        self.ui.add_feedback_message("Ressourcen verkauft!", feedback_type="success")
                                    elif building_type == "Raffinerie":
                                        self.ui.add_feedback_message("Öl zu Kraftstoff verarbeitet!", feedback_type="success")
                                    elif building_type == "Ölplattform":
                                        self.ui.add_feedback_message("Öl abgeholt!", feedback_type="success")
                                    elif building_type == "Kobaltanreicherung":
                                        self.ui.add_feedback_message("Kobalt verarbeitet!", feedback_type="success")
                                    else:
                                        self.ui.add_feedback_message(f"Mit {building_type} interagiert!", feedback_type="success")
                            else:
                                if obj["type"] == "resource":
                                    resource_type = obj["object"].type
                                    self.ui.add_feedback_message(f"Benötige Gebäude für {resource_type}!", feedback_type="error")
                                elif obj["type"] == "building":
                                    building_type = obj["object"].type
                                    if building_type == "Markt":
                                        self.ui.add_feedback_message("Keine Ressourcen zum Verkaufen!", feedback_type="warning")
                                    elif building_type == "Raffinerie":
                                        self.ui.add_feedback_message("Benötige Öl im Frachtraum!", feedback_type="warning")
                                    elif building_type == "Ölplattform":
                                        self.ui.add_feedback_message("Frachtraum voll!", feedback_type="warning")
                                    elif building_type == "Kobaltanreicherung":
                                        self.ui.add_feedback_message("Benötige Kobalt im Frachtraum!", feedback_type="warning")
                                    else:
                                        self.ui.add_feedback_message("Interaktion nicht möglich!", feedback_type="error")
                        else:
                            self.ui.add_feedback_message("Nichts in der Nähe!", feedback_type="info")
            
            # Mausklicks
            elif event.type == pygame.MOUSEBUTTONDOWN:
                if self.game_state == "playing" and (self.ui.building_menu_open or self.ui.building_mode or self.ui.ship_inventory_open or self.ui.building_interaction_open):
                    # UI übernimmt Mausklick
                    clicked_result = self.ui.handle_mouse_click(pygame.mouse.get_pos(), event.button)
                    
                    # Erfolgreiche Gebäudeplatzierung (nur anzeigen wenn wirklich gebaut wurde)
                    if clicked_result and self.ui.building_mode:
                        # Wenn wir hier sind, wurde ein Mausklick im Baumodus verarbeitet
                        # Aber wir wissen noch nicht, ob ein Gebäude platziert wurde
                        
                        # Prüfe, ob der Baumodus beendet wurde (was bedeutet, dass ein Gebäude platziert wurde)
                        if not self.ui.building_mode and self.ui.building_type_to_place is None:
                            # Gebäude wurde platziert
                            self.ui.add_feedback_message(f"{self.ui.selected_building_type} platziert!", feedback_type="success")
                            # Deaktivieren, da wir die Auswahl bereits zurückgesetzt haben
                            self.ui.selected_building_type = None
                        elif not clicked_result:
                            # Klick war nicht erfolgreich (z.B. ungültige Position)
                            self.ui.add_feedback_message("Gebäude kann hier nicht platziert werden", feedback_type="error")
            
            # Mausbewegung für Kamera
            elif event.type == pygame.MOUSEMOTION and self.game_state == "playing" and not (self.ui.building_menu_open or self.ui.ship_inventory_open or self.ui.building_interaction_open):
                self.player.handle_mouse_movement(event.rel)
            
            # Mausposition für UI aktualisieren
            self.ui.cursor_pos = pygame.mouse.get_pos()
    
    def update(self):
        """Aktualisiert den Spielstatus"""
        # Zeitdifferenz berechnen
        self.dt = self.clock.tick(60) / 1000.0  # In Sekunden
        
        # Aktueller Status der Tastatur
        keys = pygame.key.get_pressed()
        
        # Nur aktualisieren, wenn im Spielmodus
        if self.game_state == "playing":
            # Spielerobjekte aktualisieren
            self.player.move(self.dt, keys)
            
            # Welt aktualisieren
            self.world.update(self.dt)
            
            # Ressourcenmanager aktualisieren
            self.resource_manager.update(self.dt)
            
            # UI-Feedback-Nachrichten aktualisieren
            self.ui.update_feedback_messages(self.dt)
    
    def render(self):
        self.screen.fill(BLUE)  # Hintergrund (Wasser)
        
        if self.game_state == "playing" or self.game_state == "paused":
            # Welt rendern
            self.world.render(self.screen, self.player)
            
            # UI rendern
            self.ui.render(self.game_state)
            
        elif self.game_state == "menu":
            # Menü rendern
            self.ui.render_menu()
        
        pygame.display.flip()
    
    def run(self):
        while self.running:
            self.handle_events()
            self.update()
            self.render()
        
        pygame.quit()
        sys.exit()

if __name__ == "__main__":
    game = Game()
    game.run() 