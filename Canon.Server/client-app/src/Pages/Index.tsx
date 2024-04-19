import {AppBar, Button, Grid, Toolbar, Typography} from "@mui/material";
import {InputField} from "./InputField.tsx";
import {CSSProperties, useState} from "react";
import {OutputField} from "./OutputField.tsx";
import createClient from "openapi-fetch";
import * as openapi from '../openapi';

const client = createClient<openapi.paths>();

interface outputData {
    compiledCode: string,
    id: string,
    imageAddress: string,
    sourceCode: string
}

export function Index() {

    const [inputValue, setInputValue] = useState('');
    const [outputValue, setOutputValue] = useState<outputData>({
        compiledCode: "",
        sourceCode: "",
        id: "",
        imageAddress: ""
    });
    const handleValueChange = (value: string) => {
        setInputValue(value);
    };


    async function compilerButtonClick() {
        console.log(inputValue)
        const {data} = await client.POST("/api/Compiler", {
            body: {
                code: inputValue
            }
        })
        console.log(data)
        if (data != undefined) {
            setOutputValue({
                compiledCode: data.compiledCode,
                sourceCode: data.sourceCode,
                id: data.id,
                imageAddress: data.imageAddress
            })
        }
    }

    return <>
        <div className={"title"}
             style={titleClassCss}>
            <AppBar style={{zIndex: 0}}>
                <Toolbar style={{width: '100%'}}>
                    <Typography variant="h6">
                        任昌骏组编译器
                    </Typography>
                    <Button style={{
                        position: "absolute",
                        left: "50%",
                        transform: "translateX(-50%)",
                        fontSize: "medium",
                    }}
                            variant="outlined"
                            color="inherit"
                            onClick={compilerButtonClick}
                    >
                        编译
                    </Button>
                </Toolbar>
            </AppBar>
        </div>

        <div className={"content"}
             style={contentClassCss}>
            <Grid container spacing={2} style = {{width: "100%",height: "100%"}}>
                <Grid item xs={12} sm={6} style = {{width: "100%",height: "100%"}}>
                    <InputField onValueChange={handleValueChange}>
                    </InputField>
                </Grid>
                <Grid item xs={12} sm={6} style = {{width: "100%",height: "100%"}}>
                    <OutputField imgPath={outputValue.imageAddress}>
                    </OutputField>
                </Grid>
            </Grid>
        </div>
    </>
}

const titleClassCss: CSSProperties = {
    position: "absolute",
    height: "max-content",
    width: "100%",
}
const contentClassCss: CSSProperties = {
    position: "relative",
    height: "88%",
    top: "12%",
    width: "100%",
}
