// Конфигурация приложения
const AppConfig = {
    // URL API сервера
    apiBaseUrl: 'https://localhost:7051/api',
    
    // Настройки токенов
    tokenKey: 'access_token',
    refreshTokenKey: 'refresh_token',
    
    // Форматы
    dateFormat: 'ru-RU',
    currencyFormat: {
        style: 'currency',
        currency: 'RUB',
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    },
    
    // Уведомления
    notificationTimeout: 3000
};

// Экспорт для использования в других модулях
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AppConfig;
}

