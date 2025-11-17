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
            
            // Сохраняем токен в localStorage
            if (response && response.accessToken) {
                localStorage.setItem('access_token', response.accessToken);
                if (response.refreshToken) {
                    localStorage.setItem('refresh_token', response.refreshToken);
                }
                console.log('✅ Токен сохранён в localStorage');
            }
            
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
            
            // Сохраняем токен в localStorage
            if (response && response.accessToken) {
                localStorage.setItem('access_token', response.accessToken);
                if (response.refreshToken) {
                    localStorage.setItem('refresh_token', response.refreshToken);
                }
                console.log('✅ Токен сохранён в localStorage');
            }
            
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
            
            // Очищаем токены из localStorage
            localStorage.removeItem('access_token');
            localStorage.removeItem('refresh_token');
            console.log('✅ Токены удалены из localStorage');
            
            window.location.href = '/Account/Login';
        } catch (error) {
            console.error('Logout error:', error);
            
            // В любом случае очищаем и перенаправляем
            localStorage.removeItem('access_token');
            localStorage.removeItem('refresh_token');
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

