# ERO — Founder Creator & Presentation Foundation

## Objectif
Cette structure est intégrée au socle ERO dès maintenant afin d'éviter une seconde implémentation ultérieure.

## Founder Creator
Entitlement serveur : `FOUNDER_CREATOR`

- réservé au compte créateur/admin autorisé ;
- aucun paiement réel requis pour tester les contenus normalement premium ;
- accès de test aux fonctionnalités Premium ;
- accès aux 3 Season Pass Premium pour tests ;
- possibilité de tester les anciennes saisons ;
- activation/désactivation côté serveur ;
- non transférable ;
- non duplicable ;
- jamais attribué par une mécanique client ;
- journalisation des usages administratifs ;
- ne constitue pas un avantage commercial destiné aux joueurs.

## Architecture de présentation
Les systèmes de gameplay doivent exposer leurs données à une couche de présentation commune afin que l'UI finale ne soit pas reconstruite deux fois.

Prévoir dès le départ :
- Character Presentation : modèle, silhouette, équipement visible, arme, effets ;
- Equipment Presentation : apparence séparée des statistiques quand possible ;
- PET Presentation : 25 PET, 5 évolutions, forme SSR V humanoïde ;
- Class Presentation : 7 classes définitives et identité visuelle propre ;
- Skill Presentation : icône, nom, description, niveau, prérequis, état actif/inactif ;
- Tower Presentation : 5 Tours, étage, récompense, classement ;
- Season Pass Presentation : Aventurier / Maître des Tours / Maître de l'Arène, Free/Premium ;
- Inventory Presentation : catégories, recherche, tri, verrouillage, poids, équipement ;
- Forge Presentation : qualité, niveau, matériaux, progression, résultats ;
- Guild Presentation : niveau, membres, Gear Score, boutique et tickets ;
- World Presentation : carte, régions, météo, événements, MVP, World Boss ;
- Codex Presentation : cartes, découvertes, secrets, achievements et collections.

## Direction artistique
La présentation doit viser un MMORPG fantasy moderne, lisible et premium sans copier les assets ou interfaces propriétaires d'autres jeux.

Principes :
- silhouettes immédiatement reconnaissables ;
- couleurs et effets cohérents par classe/élément/rareté ;
- Common → Uncommon → Rare → Epic → Legendary → Mythic → Unique ;
- animations et VFX proportionnés au niveau de rareté ;
- interface responsive PC en priorité, extensible aux autres écrans ;
- FR/EN dès la première version ; système extensible à d'autres langues ;
- assets originaux ou légalement licenciés uniquement.

## UI à prévoir dans le socle
- HUD combat ;
- barre de compétences multi-barres ;
- personnage ;
- équipement ;
- inventaire ;
- PET ;
- invocation ;
- Forge ;
- carte ;
- quêtes ;
- Codex ;
- guildes ;
- boutique/monnaies ;
- Tours ;
- Season Pass ;
- PvP/rankings ;
- notifications ;
- social ;
- paramètres/localisation.

## Règle d'intégration
Les systèmes backend/gameplay doivent fournir des modèles de données stables et des événements de présentation. Les écrans UI consomment ces contrats au lieu de réimplémenter les règles métier.

Cela permet de construire gameplay, UI, VFX, assets et présentation ensemble et d'éviter les doublons lors de la phase de finition.
