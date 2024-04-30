import {AppBar, Button, Grid, Toolbar, Typography} from "@mui/material";
import {InputField} from "./InputField.tsx";
import {CSSProperties, useEffect, useState} from "react";
import {OutputField} from "./OutputField.tsx";
import createClient from "openapi-fetch";
import * as openapi from '../openapi';
import {enqueueSnackbar} from "notistack";
import {useNavigate} from "react-router-dom";
import {HistoryPage} from "./HistoryPage.tsx";

const client = createClient<openapi.paths>();


export function Index() {

    const [inputValue, setInputValue] = useState('');
    const [outputValue, setOutputValue] = useState<openapi.components["schemas"]["CompileResponse"]>({
        compiledCode: "",
        sourceCode: "",
        id: "",
        imageAddress: "",
        compileTime: "",
        compileInformation: ""
    });
    const [historyPageState,setHistoryPageState] = useState(false);
    const navigate = useNavigate(); // 跳转hook

    useEffect(() => {
        // 进入页面的初始化
        const path = location.pathname.substring(1);
        if (path === "") {
            setInputValue("");
            setOutputValue({
                compiledCode: "",
                sourceCode: "",
                id: "",
                imageAddress: "pic/uncompiled.png",
                compileTime: "",
                compileInformation: ""
            })
            return;
        }
        const getCompileInstance = async () => {
            const {data} = await client.GET("/api/Compiler/{compileId}", {
                params:
                    {
                        path:
                            {
                                compileId: path
                            }
                    }
            })
            if (data !== undefined) {
                setInputValue(data.sourceCode);
                setOutputValue(data)
            }
        }
        getCompileInstance();
    }, []);


    const handleValueChange = (value: string) => {
        setInputValue(value);
    };


    async function compilerButtonClick() {

        const {data} = await client.POST("/api/Compiler", {
            body: {
                code: inputValue
            }
        })

        if (data !== undefined) {
            setOutputValue(data);
            enqueueSnackbar("编译成功", {variant: "success", anchorOrigin: {vertical: 'bottom', horizontal: 'right'}});
            navigate(`/${data.id}`, {})

        } else {
            // error
            enqueueSnackbar("编译失败", {variant: "error", anchorOrigin: {vertical: 'bottom', horizontal: 'right'}});
        }
    }

    function historyButtonClick() {
        setHistoryPageState(true);
    }

    return <>
        <div className={"title"}
             style={titleClassCss}>
            <AppBar style={{zIndex: 0}}>
                <Toolbar style={{width: '100%'}}>
                    <Typography variant="h6">
                        Canon
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

                    <Button style={{
                        position: "absolute",
                        right: "10%",
                        fontSize: "medium",
                    }}
                            variant="text"
                            color="inherit"
                            onClick={historyButtonClick}>
                            历史记录
                    </Button>
                </Toolbar>
            </AppBar>
        </div>

        <div className={"content"}
             style={contentClassCss}>
            <Grid container spacing={2} style={{width: "100%", height: "100%"}}>
                <Grid item xs={12} sm={6} style={{width: "100%", height: "100%"}}>
                    <InputField defaultValue={inputValue} onValueChange={handleValueChange}>
                    </InputField>
                </Grid>
                <Grid item xs={12} sm={6} style={{width: "100%", height: "100%"}}>
                    <OutputField data={outputValue}>
                    </OutputField>
                </Grid>
            </Grid>
        </div>
        <HistoryPage state = {historyPageState} setState={setHistoryPageState}>

        </HistoryPage>
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
