import React, {useState, useEffect, useRef} from 'react';
import axios from 'axios';
import './App.css'; // Import the CSS file
const serverUrl = (window).SERVER_URL;

interface Message {
    sender: 'user' | 'bot';
    text: string;
}

interface Persona {
    name: string;
    greeting: string;
    conversationStyle: string;
    farewell: string;
}

const App: React.FC = () => {
    const [messages, setMessages] = useState<Message[]>([]);
    const [input, setInput] = useState<string>('');
    const [currentPersona, setCurrentPersona] = useState<Persona | null>(null);
    const inputRef = useRef<HTMLInputElement | null>(null);
    const chatBoxRef = useRef<HTMLDivElement | null>(null);

    useEffect(() => {
        // Focus the input when the component mounts
        if(inputRef.current !== null) inputRef.current!.focus();
    }, []);

    useEffect(() => {
        console.log("...")
        console.log(chatBoxRef.current);
        const inputRefCurrent = inputRef.current;
        if(inputRefCurrent) inputRefCurrent.focus();
        if (chatBoxRef.current) {
            if ("scrollHeight" in chatBoxRef.current) {
                chatBoxRef.current.scrollTo(0, 100000)
            }
        }
    }, [messages]);


    const sendMessage = async () => {
        if (input.trim() === '') return;

        const userMessage: Message = { sender: 'user', text: input };
        setMessages(prevMessages => [...prevMessages, userMessage]);
        setInput('');

        try {
            const response = await axios.post<{ reply: string, persona?: Persona }>(`${serverUrl}/chat`, { text: input });
            const botMessage: Message = { sender: 'bot', text: response.data.reply };
            setMessages(prevMessages => [...prevMessages, botMessage]);

            if (response.data.persona) {
                setCurrentPersona(response.data.persona);
            }
        } catch (error) {
            console.error('Error contacting backend', error);
        }
    };

    const handleKeyDown = (event: React.KeyboardEvent) => {
        if (event.key === 'Enter' && event.ctrlKey) {
            sendMessage();
        }
    };

    return (
        <div className="App">
            <h1>Chatbot</h1>
            {currentPersona && (
                <div className="current-persona">
                    <p>Current Persona: <strong>{currentPersona.name}</strong></p>
                    <p><em>{currentPersona.greeting}</em></p>
                </div>
            )}
            <div className="chat-box" ref={chatBoxRef}
            >
                {messages.map((msg, index) => (
                    <div

                        key={index}
                        className={`message ${msg.sender === 'user' ? 'user-message' : 'bot-message'}`}
                    >
                        {msg.text}
                    </div>
                ))}
            </div>
            <input
                ref={inputRef}
                value={input}
                onChange={(e) => setInput(e.target.value)}
                onKeyDown={handleKeyDown}
                className="chat-input"
            />
            <button onClick={sendMessage} className="send-button">Send</button>
        </div>
    );
};

export default App;