/**
 * Модуль для работы с аутентификацией
 */
class AuthService {
    constructor(apiClient) {
        this.api = apiClient;
    }

    /**
     * Вход в систему
     * @param {string} email 
     * @param {string} password 
     * @returns {Promise<object>}
     */
    async login(email, password) {
        try {
            const response = await this.api.post('/api/auth/login', {
                email,
                password
            });
            return response;
        } catch (error) {
            throw error;
        }
    }

    /**
     * Регистрация нового пользователя
     * @param {object} data - {email, password, fullName, phone}
     * @returns {Promise<object>}
     */
    async register(data) {
        try {
            const response = await this.api.post('/api/auth/register', {
                email: data.email,
                password: data.password,
                fullName: data.fullName,
                phone: data.phone || ''
            });
            return response;
        } catch (error) {
            throw error;
        }
    }

    /**
     * Выход из системы
     * @returns {Promise<void>}
     */
    async logout() {
        try {
            await this.api.post('/api/auth/logout', {});
            window.location.href = '/Account/Login';
        } catch (error) {
            console.error('Logout error:', error);
            // В любом случае перенаправляем на логин
            window.location.href = '/Account/Login';
        }
    }

    /**
     * Проверка статуса аутентификации
     * @returns {Promise<object>}
     */
    async checkAuth() {
        try {
            const user = await this.api.get('/api/auth/me');
            return user;
        } catch (error) {
            // Пробрасываем ошибку дальше для правильной обработки
            throw error;
        }
    }

    /**
     * Обновление токенов
     * @returns {Promise<object>}
     */
    async refreshToken() {
        try {
            const response = await this.api.post('/api/auth/refresh', {});
            return response;
        } catch (error) {
            throw error;
        }
    }
}

// Создаем глобальный экземпляр
window.authService = new AuthService(window.apiClient);

