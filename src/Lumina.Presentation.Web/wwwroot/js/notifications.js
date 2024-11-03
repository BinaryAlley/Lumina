/**
 * Enum-like object for notification types.
 * @readonly
 * @enum {string}
 */
const NotificationType = {
    INFO: 'info',
    SUCCESS: 'success',
    WARNING: 'warning',
    ERROR: 'error'
};

/**
 * Manages the creation, display, and removal of notifications.
 */
class NotificationService {
    /**
     * Creates an instance of NotificationService.
     * Initializes the notification container and appends it to the document body.
     */
    constructor() {
        this.notifications = [];
        this.container = document.createElement('div');
        this.container.className = 'notification-container';
        document.body.appendChild(this.container);
    }

    /**
     * Displays a new notification.
     * @param {string} message - The message to display in the notification.
     * @param {NotificationType} type - The type of notification.
     */
    show(message, type) {
        const notification = {
            id: Date.now().toString(),
            message,
            type
        };

        this.addNotification(notification);
    }

    /**
     * Adds a notification to the list and displays it.
     * @param {Object} notification - The notification object to add.
     * @param {string} notification.id - Unique identifier for the notification.
     * @param {string} notification.message - The notification message.
     * @param {NotificationType} notification.type - The type of notification.
     */
    addNotification(notification) {
        this.notifications.push(notification);
        this.displayNotification(notification);

        // log the notification to the console
        const message = `${notification.type.charAt(0).toUpperCase() + notification.type.slice(1)}: ${notification.message}`;
        if (notification.type === 'info')
            console.info(message);
        else if (notification.type === 'warning')
            console.warn(message);
        else if (notification.type === 'error')
            console.error(message);
        else
            console.log(message);

        // set a timeout to remove the notification after 10 seconds (fading out effect is handled in notification.css)
        setTimeout(() => this.removeNotification(notification.id), 10000);
    }

    /**
     * Removes a notification from the list and the DOM.
     * @param {string} id - The ID of the notification to remove.
     */
    removeNotification(id) {
        const index = this.notifications.findIndex(n => n.id === id);
        if (index !== -1) {
            this.notifications.splice(index, 1);
            const element = document.getElementById(`notification-${id}`);
            if (element) 
                element.remove();
        }
    }

    /**
     * Creates and displays a notification element in the DOM.
     * @param {Object} notification - The notification object to display.
     * @param {string} notification.id - Unique identifier for the notification.
     * @param {string} notification.message - The notification message.
     * @param {NotificationType} notification.type - The type of notification.
     */
    displayNotification(notification) {
        const notificationElement = document.createElement('div');
        notificationElement.id = `notification-${notification.id}`;
        notificationElement.className = `notification ${notification.type}`;
        notificationElement.innerHTML = `
            <div class="notification-shine-effect"></div>
            <div class="notification-body">
                <span class="notification-message">${notification.message}</span>
                <button class="close-button" onclick="notificationService.removeNotification('${notification.id}')">×</button>
            </div>
        `;
        this.container.appendChild(notificationElement);
    }
}

// create a global instance of the NotificationService
const notificationService = new NotificationService();
