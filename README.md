# MOBA Prototype

Небольшой прототип в жанре MOBA на Unity **6000.0.29f1**: одна игровая
сессия от старта до победы одной из команд, с подведением итогов.

## Как запустить

1. Открыть проект в Unity Hub (версия **6000.0.29f1**).
2. Открыть сцену `Assets/Scenes/Game.unity`.
3. Нажать Play.

## Управление

| Действие        | Клавиша / кнопка        |
|-----------------|--------------------------|
| Движение        | WASD / стрелки           |
| Атака           | ЛКМ (по ближайшему врагу в радиусе) |

## Игровой цикл

- Обе команды (Ally / Enemy) с интервалом высылают волны крипов (`SpawnManager`),
  которые идут по линии к вражеской базе и атакуют всё, что встречают на пути.
- У каждой команды есть башня (`TowerAttack`) и база (`Health`, 500 HP).
- Игрок управляет героем команды **Ally** и может атаковать вручную.
- Матч заканчивается, когда одна из баз уничтожена — `GameManager` фиксирует
  победителя и показывает экран итогов (время матча, убийства и урон по
  каждой команде), после чего можно перезапустить сессию кнопкой Restart.

## Архитектура

```
Assets/
├── Scenes/Game.unity        # единственная сцена прототипа
├── Scripts/
│   ├── Core/                # Health, TeamId/UnitType
│   ├── Player/               # PlayerMovement, PlayerCombat
│   ├── CameraControl/         # CameraFollow
│   ├── Units/                 # LaneUnitAI (крипы)
│   ├── Towers/                 # TowerAttack
│   ├── Projectiles/             # Projectile
│   ├── Managers/                 # GameManager, SpawnManager
│   └── UI/                       # UIManager (HUD + экран итогов строится кодом)
├── Prefabs/
│   ├── Allies/AllyCreep.prefab
│   ├── Enemies/EnemyCreep.prefab
│   └── Projectiles/Projectile.prefab
└── Materials/                 # Ally/Enemy/Neutral/Ground/Lane
```

Иерархия сцены разложена по контейнерам: `Environment`, `Bases`, `Towers`,
`Allies`, `Enemies`, `Projectiles`, `Managers`, `SpawnPoints`, `Canvas` —
все заспавненные юниты попадают в соответствующий контейнер
(`Allies`/`Enemies`) через `SpawnManager`.

## Статистика матча

`GameManager` считает по ходу сессии:
- количество побеждённых юнитов каждой командой;
- суммарный нанесённый урон каждой командой;
- время матча.

Всё это выводится на экране результатов в конце сессии.
