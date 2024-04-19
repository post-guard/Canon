import {TextField} from "@mui/material";
import {CSSProperties, useState} from "react";


// @ts-expect-error ...
export function InputField({ onValueChange }) {

    const [inputValue, setInputValue] = useState('');

    // @ts-expect-error ...
    const handleChange = (e) => {
        const newValue = e.target.value;
        setInputValue(newValue);
        onValueChange(newValue);
    };


    return <>
        <div className={"input-field"} style={inputFieldClassCss}>
            <TextField
                id="outlined-textarea"
                label="Pascal Code"
                rows={24}
                multiline
                style={
                    {
                        width: "100%",
                        height: "100%"
                    }
                }
                value={inputValue}
                onChange={handleChange}
            />
        </div>
    </>
}

const inputFieldClassCss: CSSProperties = {
    width: "100%",
    height: "100%",
    padding: "5% 5%",
    boxSizing: "border-box"
}
