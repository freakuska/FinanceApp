/**
 * Базовый клиент для работы с API
 */
class ApiClient {
    constructor(baseUrl) {
        this.baseUrl = baseUrl || 'https://localhost:7051';
    }

    /**
     * Выполняет HTTP запрос к API
     * @param {string} endpoint - путь к endpoint (например '/api/auth/login')
     * @param {object} options - опции fetch
     * @returns {Promise<any>}
     */
    async request(endpoint, options = {}) {
        const url = `${this.baseUrl}${endpoint}`;
        
        const defaultOptions = {
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include', // Важно для cookies
        };

        const config = {
            ...defaultOptions,
            ...options,
            headers: {
                ...defaultOptions.headers,
                ...options.headers,
            },
        };

        try {
            const response = await fetch(url, config);

            // Если 401 - перенаправляем на логин (только если мы не на странице логина)
            if (response.status === 401) {
                if (!window.location.pathname.includes('/Account/')) {
                    window.location.href = '/Account/Login';
                }
                throw new Error('Unauthorized');
            }

            // Пробуем распарсить JSON
            const contentType = response.headers.get('content-type');
            let data = null;
            
            if (contentType && contentType.includes('application/json')) {
                data = await response.json();
            }

            if (!response.ok) {
                const error = new Error(data?.message || `HTTP error! status: ${response.status}`);
                error.status = response.status;
                error.data = data;
                throw error;
            }

            return data;
        } catch (error) {
            // Если это сетевая ошибка (API недоступен), не перенаправляем
            if (error.message === 'Failed to fetch' || error.name === 'TypeError') {
                console.error('API недоступен:', error);
                error.isNetworkError = true;
            } else {
                console.error('API request error:', error);
            }
            throw error;
        }
    }

    /**
     * GET запрос
     */
    async get(endpoint, options = {}) {
        return this.request(endpoint, {
            ...options,
            method: 'GET',
        });
    }

    /**
     * POST запрос
     */
    async post(endpoint, body, options = {}) {
        return this.request(endpoint, {
            ...options,
            method: 'POST',
            body: JSON.stringify(body),
        });
    }

    /**
     * PUT запрос
     */
    async put(endpoint, body, options = {}) {
        return this.request(endpoint, {
            ...options,
            method: 'PUT',
            body: JSON.stringify(body),
        });
    }

    /**
     * DELETE запрос
     */
    async delete(endpoint, options = {}) {
        return this.request(endpoint, {
            ...options,
            method: 'DELETE',
        });
    }

    /**
     * PATCH запрос
     */
    async patch(endpoint, body, options = {}) {
        return this.request(endpoint, {
            ...options,
            method: 'PATCH',
            body: JSON.stringify(body),
        });
    }
}

// Создаем глобальный экземпляр
window.apiClient = new ApiClient();

