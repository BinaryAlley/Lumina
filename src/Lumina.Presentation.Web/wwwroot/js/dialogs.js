/**
 * Enum-like object for dialog buttons.
 * @readonly
 * @enum {string}
 */
const DialogButton = {
    OK: 'ok',
    CANCEL: 'cancel',
    YES: 'yes',
    NO: 'no',
    RETRY: 'retry',
    ABORT: 'abort',
    IGNORE: 'ignore'
};

/**
 * Enum-like object for dialog icons.
 * @readonly
 * @enum {string}
 */
const DialogIcon = {
    NONE: 'none',
    INFO: 'info',
    WARNING: 'warning',
    ERROR: 'error',
    QUESTION: 'question'
};

/**
 * Enum-like object for predefined button combinations.
 * @readonly
 * @enum {Array<DialogButton>}
 */
const DialogButtons = {
    OK: [DialogButton.OK],
    OK_CANCEL: [DialogButton.OK, DialogButton.CANCEL],
    YES_NO: [DialogButton.YES, DialogButton.NO],
    YES_NO_CANCEL: [DialogButton.YES, DialogButton.NO, DialogButton.CANCEL],
    RETRY_CANCEL: [DialogButton.RETRY, DialogButton.CANCEL],
    ABORT_RETRY_IGNORE: [DialogButton.ABORT, DialogButton.RETRY, DialogButton.IGNORE]
};

/**
 * Manages the creation and display of modal dialogs.
 */
class DialogService {
    constructor() {
        this.dialogQueue = [];
        this.isDialogOpen = false;
    }

    /**
     * Shows a modal dialog and returns a Promise that resolves with the clicked button.
     * @param {string} message - The message to display in the dialog.
     * @param {string} title - The title of the dialog.
     * @param {Array<DialogButton>} buttons - Array of buttons to display.
     * @param {DialogIcon} icon - The icon to display.
     * @returns {Promise<DialogButton>} A promise that resolves with the clicked button.
     */
    show(message, title = jsLocalizedDialogs.confirmation, buttons = DialogButtons.OK, icon = DialogIcon.NONE) {
        const dialogPromise = new Promise((resolve) => {
            const dialog = {
                message,
                title,
                buttons,
                icon,
                resolve
            };

            this.dialogQueue.push(dialog);
            if (!this.isDialogOpen) 
                this.showNextDialog();
        });

        return dialogPromise;
    }

    /**
     * Shows the next dialog in the queue.
     * @private
     */
    showNextDialog() {
        if (this.dialogQueue.length === 0) {
            this.isDialogOpen = false;
            return;
        }

        this.isDialogOpen = true;
        const dialog = this.dialogQueue[0];
        this.displayDialog(dialog);
    }

    /**
     * Creates and displays a dialog element in the DOM.
     * @private
     * @param {Object} dialog - The dialog configuration object.
     */
    displayDialog(dialog) {
        const modalBackground = document.getElementById('modal-background-article');
        modalBackground.style.display = 'block';
        document.querySelector('.dialog-container').style.display = 'block';
        const dialogElement = document.createElement('div');
        dialogElement.className = 'dialog';

        dialogElement.innerHTML = `
            <div class="dialog-content">
                <div class="dialog-header">
                    <h2>${dialog.title}</h2>
                </div>
                <div class="dialog-body">
                    ${dialog.icon !== DialogIcon.NONE ? `<div class="dialog-icon ${dialog.icon}"></div>` : ''}
                    <div class="dialog-message">${dialog.message}</div>
                </div>
                <div class="dialog-buttons">
                ${dialog.buttons.map(button =>
                    `<button class="dialog-button ${button === 'yes' || button === 'ok' ? 'confirm-button' : 'abort-button'}" data-button="${button}">
                        ${this.getLocalizedButtonText(button)}
                    </button>`
                ).join('')}
                </div>
            </div>
        `;
        // add a resize observer to handle content height
        const resizeObserver = new ResizeObserver(() => {
            const modalHeight = modalBackground.clientHeight;
            const dialogContent = dialogElement.querySelector('.dialog-content');
            dialogContent.style.maxHeight = `${modalHeight - 50}px`;
            dialogContent.style.overflowY = 'auto';
        });

        resizeObserver.observe(modalBackground);

        // cleanup observer when dialog is closed
        const cleanup = () => {
            resizeObserver.disconnect();
            document.removeEventListener('keydown', keyHandler);
        };

        // add click handlers for buttons
        const buttons = dialogElement.querySelectorAll('.dialog-button');
        buttons.forEach(button => {
            button.addEventListener('click', () => {
                cleanup();
                this.handleButtonClick(button.dataset.button);
            });
        });

        // add keyboard support
        const keyHandler = (e) => {
            if (e.key === 'Escape' && dialog.buttons.includes(DialogButton.CANCEL)) {
                cleanup();
                this.handleButtonClick(DialogButton.CANCEL);
            }
            if (e.key === 'Enter' && dialog.buttons.includes(DialogButton.OK)) {
                cleanup();
                this.handleButtonClick(DialogButton.OK);
            }
        };
        document.addEventListener('keydown', keyHandler);

        document.querySelector('.dialog-container').appendChild(dialogElement);
    }

    /**
     * Handles button clicks and resolves the dialog promise.
     * @private
     * @param {DialogButton} buttonType - The type of button clicked.
     */
    handleButtonClick(buttonType) {
        const currentDialog = this.dialogQueue.shift();
        const dialogElement = document.querySelector('.dialog-container').querySelector('.dialog');

        if (dialogElement) 
            dialogElement.remove();

        if (this.dialogQueue.length === 0) {
            const modalBackground = document.getElementById('modal-background-article');
            modalBackground.style.display = 'none';
            document.querySelector('.dialog-container').style.display = 'none';
        }

        currentDialog.resolve(buttonType);
        this.showNextDialog();
    }

    /**
     * Gets the localized text for a button type.
     * @private
     * @param {DialogButton} buttonType - The button type.
     * @returns {string} The localized button text.
     */
    getLocalizedButtonText(buttonType) {
        return jsLocalizedDialogs[buttonType.toLowerCase()] || buttonType;
    }
}

// create a global instance of the DialogService
const dialogService = new DialogService();
