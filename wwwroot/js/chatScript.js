class AICaller {

    last_is_success;

    last_return;

    async post_message(relative_url, text) {

        this.last_return = "";

        this.last_is_success = false;

        return window.fetch(relative_url, {

            method: "POST",

            headers: {

                "RequestVerificationToken": this.#getCookie('RequestVerificationToken'),

                "Content-Type": "application/json"
            },

            body: JSON.stringify({ text: text, chatHistory: this.#chatHistory })

        }).then((response) => {

            if (!response.ok) return `ERROR ${response.statusText} ${response.status}`;

            return response.json();

        }).then((data) => {

            if (!Object.hasOwn(data, "text") || data.text.startsWith("ERROR")) {

                this.last_return = "ERROR " + data.text;

                this.last_is_success = false;

            } else {

                this.last_is_success = true;

                this.last_return = data.text;

                this.#chatHistory = data.chats;
            }
        });
    }

    /*---- private methods and data ----*/

    /*  chat history is cached as a serialized string
        it is sent along with every request (see window.fetch call above)
    */
    #chatHistory;

    #getCookie(name) {

        var value = "; " + document.cookie;

        var parts = value.split("; " + name + "=");

        if (parts.length == 2) return parts.pop().split(";").shift();

    };
}