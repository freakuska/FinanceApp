/**
 * Модуль для работы с финансовыми операциями
 */
class OperationsService {
    constructor(apiClient) {
        this.api = apiClient;
    }

    /**
     * Получить список операций с фильтрацией
     * @param {object} filter - {startDate, endDate, type, currency, tagIds, page, pageSize}
     * @returns {Promise<object>}
     */
    async getOperations(filter = {}) {
        try {
            const params = new URLSearchParams();
            
            if (filter.startDate) params.append('startDate', filter.startDate);
            if (filter.endDate) params.append('endDate', filter.endDate);
            if (filter.type) params.append('type', filter.type);
            if (filter.currency) params.append('currency', filter.currency);
            if (filter.page) params.append('page', filter.page);
            if (filter.pageSize) params.append('pageSize', filter.pageSize);
            if (filter.tagIds && filter.tagIds.length > 0) {
                filter.tagIds.forEach(id => params.append('tagIds', id));
            }

            const queryString = params.toString();
            const endpoint = queryString ? `/api/operations?${queryString}` : '/api/operations';
            
            return await this.api.get(endpoint);
        } catch (error) {
            console.error('Get operations error:', error);
            throw error;
        }
    }

    /**
     * Получить операцию по ID
     * @param {string} id 
     * @returns {Promise<object>}
     */
    async getOperation(id) {
        try {
            return await this.api.get(`/api/operations/${id}`);
        } catch (error) {
            console.error('Get operation error:', error);
            throw error;
        }
    }

    /**
     * Создать новую операцию
     * @param {object} data - {type, amount, currency, paymentMethod, description, notes, operationDateTime, tagIds}
     * @returns {Promise<object>}
     */
    async createOperation(data) {
        try {
            return await this.api.post('/api/operations', {
                type: data.type,
                amount: parseFloat(data.amount),
                currency: data.currency || 'RUB',
                paymentMethod: data.paymentMethod,
                description: data.description || '',
                notes: data.notes || '',
                operationDateTime: data.operationDateTime || new Date().toISOString(),
                tagIds: data.tagIds || []
            });
        } catch (error) {
            console.error('Create operation error:', error);
            throw error;
        }
    }

    /**
     * Обновить операцию
     * @param {string} id 
     * @param {object} data 
     * @returns {Promise<object>}
     */
    async updateOperation(id, data) {
        try {
            return await this.api.put(`/api/operations/${id}`, data);
        } catch (error) {
            console.error('Update operation error:', error);
            throw error;
        }
    }

    /**
     * Удалить операцию
     * @param {string} id 
     * @returns {Promise<void>}
     */
    async deleteOperation(id) {
        try {
            return await this.api.delete(`/api/operations/${id}`);
        } catch (error) {
            console.error('Delete operation error:', error);
            throw error;
        }
    }

    /**
     * Получить статистику по операциям
     * @param {string} startDate 
     * @param {string} endDate 
     * @returns {Promise<object>}
     */
    async getStats(startDate, endDate) {
        try {
            const params = new URLSearchParams({
                startDate: startDate,
                endDate: endDate
            });
            
            return await this.api.get(`/api/operations/stats?${params.toString()}`);
        } catch (error) {
            console.error('Get stats error:', error);
            throw error;
        }
    }
}

// Создаем глобальный экземпляр
window.operationsService = new OperationsService(window.apiClient);

