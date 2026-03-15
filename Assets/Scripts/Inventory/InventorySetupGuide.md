# Inventory Setup Guide

## Что уже реализовано
- Инвентарь на 30 слотов.
- По умолчанию открыто 15 слотов.
- Остальные слоты разблокируются по кнопке за монеты.
- Цена разблокировки настраивается в `InventoryConfig`.
- Добавление монет, патронов, случайного предмета, удаление случайного предмета.
- Случайный выстрел с проверкой пары оружие + патроны.
- Пересчет общего веса.
- Автоматическое обновление UI после любой операции.
- Вывод сообщений в `Debug.Log` и `Debug.LogError`.

## Какие ассеты создать
1. `InventoryConfig`
2. `InventoryItemCatalog`
3. `InventoryItemDefinition` для каждого предмета:
   - Патроны пистолета
   - Патроны автомата
   - Пистолет
   - Автомат
   - Куртка
   - Бронежилет
   - Кепка
   - Шлем

## Как заполнить предметы

### Патроны пистолета
- `Category`: `Ammo`
- `AmmoType`: `Pistol`
- `WeightKg`: `0.01`
- `MaxStack`: `50`

### Патроны автомата
- `Category`: `Ammo`
- `AmmoType`: `Rifle`
- `WeightKg`: `0.01`
- `MaxStack`: `50`

### Пистолет
- `Category`: `Weapon`
- `AmmoType`: `Pistol`
- `WeightKg`: `1`
- `Damage`: `10`

### Автомат
- `Category`: `Weapon`
- `AmmoType`: `Rifle`
- `WeightKg`: `5`
- `Damage`: `20`

### Куртка
- `Category`: `TorsoArmor`
- `WeightKg`: `1`
- `Defense`: `3`

### Бронежилет
- `Category`: `TorsoArmor`
- `WeightKg`: `10`
- `Defense`: `10`

### Кепка
- `Category`: `HeadArmor`
- `WeightKg`: `0.2`
- `Defense`: `3`

### Шлем
- `Category`: `HeadArmor`
- `WeightKg`: `1`
- `Defense`: `10`

Для всех предметов можно назначить `Icon`.

## Как собрать сцену
1. Добавь на сцену `SceneContext`.
2. Создай объект `InventoryInstaller` и повесь на него `InventoryInstaller`.
3. В `InventoryInstaller` назначь `InventoryConfig` и `InventoryItemCatalog`.
4. Создай Canvas с верхней панелью кнопок и нижней областью со скроллом.
5. На корневой объект экрана повесь `InventoryScreenController`.
6. На объект визуала повесь `InventoryView`.
7. Создай 30 объектов слотов и повесь на каждый `InventorySlotView`.
8. Все 30 `InventorySlotView` добавь в список `InventoryView -> Slot Views`.
9. Назначь кнопки в `InventoryScreenController`.

## Что должно быть в каждом слоте
- `_lockedRoot`: визуал заблокированного слота
- `_unlockButton`: кнопка разблокировки
- `_unlockPriceLabel`: цена разблокировки
- `_contentRoot`: зона с иконкой предмета
- `_iconImage`: иконка
- `_countLabel`: количество в стаке
- `_emptyRoot`: визуал пустого открытого слота
- `_slotIndexLabel`: необязательный номер слота

## Ответственность классов
- `InventoryItemDefinition`: описание одного типа предмета.
- `InventoryItemCatalog`: общий список предметов и выбор случайного предмета.
- `InventoryConfig`: общие настройки инвентаря.
- `InventoryService`: вся бизнес-логика инвентаря.
- `InventoryInstaller`: регистрация зависимостей в Zenject.
- `InventoryView`: отрисовка текста баланса, веса и списка слотов.
- `InventorySlotView`: отрисовка одного слота и обработка кнопки разблокировки.
- `InventoryScreenController`: связывает UI-кнопки с `InventoryService`.

## Что не включено
- Drag and drop между слотами.
- Попап с подробной информацией о предмете.

Под это уже можно без переделки логики добавить отдельные компоненты поверх текущего UI.
