#include<iostream>
#include<fstream>
#include <sstream>
#include <cmath>
#include <thread>
#include <cstring>
using namespace std;
int get_RandonNum(int Num);
int get_RandonLocality(int Min, int Max);

enum TestData { Randon, Locality, myData};

void RandonSample(const char *path);

void LocalitySample(const char *path);

void myDataSample(const char *path);

void getSample(const char *path);

void FIFO(const char *path, int FrameSize,TestData testData);

void OPT(const char *path, int FrameSize, TestData testData);

void ESC(const char *path, int FrameSize, TestData testData);
const int Ref_str = 350;
const int Numofmem_ref = 70000;

int Ref_str_RandonArray[Numofmem_ref];

int Ref_str_LocalityArray[Numofmem_ref];

int Ref_str_myDataArray[Numofmem_ref];
int main() {
    srand((unsigned)time(NULL));

    char* path = "..\\";
    getSample(path);
    printf("\nDataSize %d", Numofmem_ref);
    int FrameSize = 0;
    for(int i = 10 ;i <= 70 ; i+=10) {
        FrameSize = i;
        printf("\n\nFrameSize %d\n", FrameSize);
        TestData testData1 = Randon;
        TestData testData2 = Locality;
        TestData testData3 = myData;
//
        FIFO(path, FrameSize, testData1);
        OPT(path, FrameSize, testData1);
        ESC(path, FrameSize, testData1);

        //-------------------------------
        FIFO(path, FrameSize, testData2);
        OPT(path, FrameSize, testData2);
        ESC(path, FrameSize, testData2);
        //-------------------------------

        FIFO(path, FrameSize, testData3);
        OPT(path, FrameSize, testData3);
        ESC(path, FrameSize, testData3);
    }
    return 0;
}
void ESC(const char *path, int FrameSize, TestData testData){

    int Frame[FrameSize];
    for(int i = 0 ; i < FrameSize ; i ++){//初始化
        Frame[i]= -1;
    }
    string ESC;
    switch(testData){
        case Randon:
            ESC = "ESC_Randon.txt";
            break;
        case Locality:
            ESC = "ESC_Locality.txt";
            break;
        case myData:
            ESC = "ESC_myData.txt";
            break;
    }
    ofstream WriteToTXT( path + ESC);
    int pagefault = 0, status, flag = 0;
    int *referenceBits = (int *) malloc(sizeof(int) * FrameSize);
    for (int i = 0; i < FrameSize; ++i) {
        referenceBits[i] = 0;
    }

    for (int i = 0; i < Numofmem_ref; ++i) {
        WriteToTXT << i;
        WriteToTXT << "\t";
        status = 0;
        int input = 0;
        switch(testData){
            case Randon:
                input = Ref_str_RandonArray[i];
                break;
            case Locality:
                input = Ref_str_LocalityArray[i];
                break;
            case myData:
                input = Ref_str_myDataArray[i];
                break;
        }
        //先段判是是否在Frame中，
        // 若在
        //    則將referenceBits設為1，直接輸出
        // 若為初始狀態
        //    referenceBits設為1，放入Frame中
        // 若不在
        //    看referencebit為0 or 1，找維0的替換
        int temp_swap_index = 0;
        for (int j = 0; j < FrameSize; ++j) {
            if (Frame[j] == input) {
                referenceBits[j] = 1;
                status = 1;//直接輸出不做任何事
                break;
            } else if (Frame[j] == -1) {//Frame中都還未使用
                temp_swap_index = Frame[j];
                Frame[j] = input;
                referenceBits[j] = 1;
                ++pagefault;
                status = 2;
                break;
            }
        }
        if (status == 0) {
            while (1) {
                if (referenceBits[flag] == 0) {
                    temp_swap_index = Frame[flag];
                    Frame[flag] = input;
                    ++pagefault;
                    referenceBits[flag] = 1;
                    break;
                } else {
                    referenceBits[flag] = 0;
                }
                flag = (flag + 1) % FrameSize;
            }
            flag = (flag + 1) % FrameSize;
            WriteToTXT << input;
            WriteToTXT << "\t";
            for (int k = 0; k < FrameSize; ++k) {
                WriteToTXT << Frame[k];
                WriteToTXT << "\t";
            }
            WriteToTXT << "swap out : ";
            WriteToTXT << temp_swap_index;
            WriteToTXT << "\n";
        }
        else if (status == 1){
            WriteToTXT << input;
            WriteToTXT << "\n";
        }
        else if (status == 2){
            WriteToTXT << input;
            WriteToTXT << "\t";
            for (int k = 0; k < FrameSize; ++k) {
                WriteToTXT << Frame[k];
                WriteToTXT << "\t";
            }
            WriteToTXT << "swap out : ";
            WriteToTXT << temp_swap_index;
            WriteToTXT << "\n";
        }
    }

    switch(testData){
        case Randon:
            printf("\nESC\tRandon\t\tpagefault %d",pagefault);
            break;
        case Locality:
            printf("\nESC\tLocality\tpagefault %d",pagefault);
            break;
        case myData:
            printf("\nESC\tmyData\t\tpagefault %d",pagefault);
            break;
    }
    free(referenceBits);
    WriteToTXT << "pagefault : ";
    WriteToTXT << pagefault;
    WriteToTXT.close();
}

void OPT(const char *path, int FrameSize, TestData testData) {//初始化
    int element_count[Ref_str];
    int element_usestate[Ref_str];
    for(int i = 0 ;i <Ref_str ;i++){
        element_count[i] = 0;
        element_usestate[i] = 0;
    }
    //計算每一個數(0~350)出現次數
    for(int i = 0 ;i < Numofmem_ref ;i++){
        switch(testData){
            case Randon:
                element_count[Ref_str_RandonArray[i]] += 1;
                break;
            case Locality:
                element_count[Ref_str_LocalityArray[i]] += 1;
                break;
            case myData:
                element_count[Ref_str_myDataArray[i]] += 1;
                break;
        }
    }

    //初始化
    int Frame[FrameSize];
    for(int i = 0 ; i < FrameSize ; i ++){
        Frame[i] = 0;
    }

    string OPT;
    switch(testData){
        case Randon:
            OPT = "OPT_Randon.txt";
            break;
        case Locality:
            OPT = "OPT_Locality.txt";
            break;
        case myData:
            OPT = "OPT_myData.txt";
            break;
    }
    ofstream WriteToTXT( path + OPT);

    int pagefault = 0;

    for(int i = 0 ;i < Numofmem_ref; i++){
        WriteToTXT << i;
        WriteToTXT << "\t";
        int input = 0;
        switch(testData){
            case Randon:
                input = Ref_str_RandonArray[i];
                break;
            case Locality:
                input = Ref_str_LocalityArray[i];
                break;
            case myData:
                input = Ref_str_myDataArray[i];
                break;
        }
        //Ref_str_LocalityArray[0]為input範圍為 0~350 之間的數字 ，假設input為100，
        // 先檢查element_usestate[100]是否為0，
        if(element_usestate[input] == 0){//若是0則代表不在frame中
            element_usestate[input] = 1 ;//條件成立後在element_usestate[100] 設為 1，避免之後再次放入，swap out 時設為 0
            //將input 放入 Frame中，先確定要放入哪一格Frame
            // 即要替換哪一個
            int swap_index = 0;
            bool countZero = false;//預設為false ,只有找到有0的才設為true
            // 1.確認Frame中裡面的每一元素是否有個數只剩下0的，
            for(int j = 0 ;j < FrameSize ;j ++){
                if(element_count[Frame[j]] == 0){
                    swap_index = j;//記錄frame哪一個要被替換
                    countZero = true;
                }
            }
            // 2.若有個數為0，替換該元素，
            if(countZero == true){
                int temp_swap_index = Frame[swap_index];//紀錄要被替換得值
                Frame[swap_index] = input;
                element_usestate[temp_swap_index] = 0;//被替換得值 設為0 即不在 frame中
                element_count[input]--;//確定放入後將element_count[100]個數減1
                WriteToTXT << input;
                WriteToTXT << "\t";
                for(int j = 0 ;j <FrameSize ;j++) {
                    WriteToTXT << Frame[j];
                    WriteToTXT << "\t";
                }
                WriteToTXT << "zero_swap out : ";
                WriteToTXT << temp_swap_index;
                WriteToTXT << "\n";
                pagefault++ ;
            } else{// 3.如果沒有0，
                //要先判斷是否Frame為空，判斷方式為前FrameSize的輸入ex:前5次
                if(i < FrameSize){
                    int temp_swap_index = Frame[i];//紀錄要被替換得值
                    //前FrameSize的輸入若有0的判斷方式依然依序放入Frame[]中
                    Frame[i] = input;
                    element_usestate[temp_swap_index] = 0;
                    element_count[input]--;//確定放入後將element_count[100]個數減1

                    WriteToTXT << input;
                    WriteToTXT << "\t";
                    for(int j = 0 ;j <FrameSize ;j++) {
                        WriteToTXT << Frame[j];
                        WriteToTXT << "\t";
                    }
                    WriteToTXT << "small than FrameSize swap out : ";
                    WriteToTXT << temp_swap_index;
                    WriteToTXT << "\n";
                    pagefault++ ;

                } else{
                    int temp_distance[FrameSize];
                    for(int j = 0 ;j <FrameSize ;j++){
                        temp_distance[FrameSize] = 0;
                    }
                    //3.1要計算frame中每一元素下一次出現距離
                    for(int j = 0 ; j < FrameSize ; j++){//先取得frame[j]的元素
                        for(int k = i ; k < Numofmem_ref ; k++){
                            //從i往後找Ref_str_LocalityArray[i~Numofmem_ref]，直到找frame[0]的元素，
                            // k - i為距離，記錄在temp_distance[j]中，並跳出。
                            int temp = 0;
                            switch(testData){
                                case Randon:
                                    temp = Ref_str_RandonArray[k];
                                    break;
                                case Locality:
                                    temp = Ref_str_LocalityArray[k];
                                    break;
                                case myData:
                                    temp = Ref_str_myDataArray[k];
                                    break;
                            }
                            if( temp == Frame[j]){
                                temp_distance[j] = k - i;
                                break;
                            }
                        }
                    }
                    //替換最遠的
                    double Max = 0;
                    int Max_index = 0;
                    for (int j = 0; j < FrameSize ; j++) {
                        if(temp_distance[j] > Max){
                            Max = temp_distance[j];
                            Max_index = j;
                        }
                    }
                    int temp_swap_index = Frame[Max_index];//紀錄要被替換得值

                    Frame[Max_index] = input;//最遠被替換
                    element_usestate[temp_swap_index] = 0;
                    element_count[input]--;//確定放入後將element_count[100]個數減1

                    WriteToTXT << Frame[Max_index];
                    WriteToTXT << "\t";
                    for(int j = 0 ;j <FrameSize ;j++) {
                        WriteToTXT << Frame[j];
                        WriteToTXT << "\t";
                    }
                    WriteToTXT << "Max swap out : ";
                    WriteToTXT << temp_swap_index;
                    WriteToTXT << "\n";
                    pagefault++ ;
                }
            }
        } else{//為1則代表在frame中，不替換
            WriteToTXT << input;
            WriteToTXT << "\t";
            for(int j = 0 ;j <FrameSize ;j++) {
                WriteToTXT << Frame[j];
                WriteToTXT << "\t";
            }
            WriteToTXT << "in Frame";
            WriteToTXT << "\n";
        }
    }
    switch(testData){
        case Randon:
            printf("\nOPT\tRandon\t\tpagefault %d",pagefault);
            break;
        case Locality:
            printf("\nOPT\tLocality\tpagefault %d",pagefault);
            break;
        case myData:
            printf("\nOPT\tmyData\t\tpagefault %d",pagefault);
            break;
            break;
    }
    WriteToTXT << "pagefault : ";
    WriteToTXT << pagefault;
    WriteToTXT.close();
}
void FIFO(const char *path, int FrameSize , TestData testData) {
    int Frame[FrameSize];
    for(int i = 0 ; i < FrameSize ; i ++){//初始化
        Frame[i] = 0;
    }
    string FIFO;
    switch(testData){
        case Randon:
            FIFO = "FIFO_Randon.txt";
            break;
        case Locality:
            FIFO = "FIFO_Locality.txt";
            break;
        case myData:
            FIFO = "FIFO_myData.txt";
            break;
    }
    ofstream WriteToTXT( path + FIFO);
    int flag = 0;
    int pagefault = 0;
    for(int i = 0 ;i < Numofmem_ref ; i++){
        WriteToTXT << i;
        WriteToTXT << "\t";
        int input = 0;
        switch(testData){
            case Randon:
                input = Ref_str_RandonArray[i];
                break;
            case Locality:
                input = Ref_str_LocalityArray[i];
                break;
            case myData:
                input = Ref_str_myDataArray[i];
                break;
        }
        for (int j = 0; j < FrameSize; ++j) {
            if (Frame[j] == input) goto out;
        }
        ++pagefault;
        Frame[flag] = input;
        flag = (flag + 1) % FrameSize;
        WriteToTXT << input;
        WriteToTXT << "\t";
        for(int k = 0 ;k <FrameSize ;k++) {
            WriteToTXT << Frame[k];
            WriteToTXT << "\t";
        }
        WriteToTXT << "\n";
        continue;
        out:;
        WriteToTXT << input;
        WriteToTXT << "\n";
    }
    switch(testData){
        case Randon:
            printf("\nFIFO\tRandon\t\tpagefault %d",pagefault);
            break;
        case Locality:
            printf("\nFIFO\tLocality\tpagefault %d",pagefault);
            break;
        case myData:
            printf("\nFIFO\tmyData\t\tpagefault %d",pagefault);
            break;
    }

    WriteToTXT << "pagefault : ";
    WriteToTXT << pagefault;
    WriteToTXT.close();
}

void getSample(const char *path) {
    RandonSample(path);
    LocalitySample(path);
    myDataSample(path);
}
void myDataSample(const char *path) {
    //先雖機選一個 process(0~350)然後再隨機產生一個數0 ~ 50作為一個process(0~350)執行次數做多執行50次
    string Sample_myData = "Sample_myData.txt";
    ofstream fout( path + Sample_myData);
    if ( fout ) {
        int count = 0 ;
        int MaxRunTime = 5;
        while (count < Numofmem_ref){
            int process= get_RandonNum(Ref_str);//Ref_str = 350
            int RunTime = get_RandonNum(MaxRunTime); // MaxRunTime = 5;
            for(int i = count ; i < count + RunTime ; i ++ ){
                Ref_str_myDataArray[i] = process;
                fout << process;
                fout << "\t";
            }
            count += RunTime;
        }
        printf("success Sample_myData.txt\n");
        fout.close();
    } else{
        printf("fail Sample_myData.txt\n");
    }
}

void LocalitySample(const char *path) {
    string Sample_Locality = "Sample_Locality.txt";
    ofstream fout( path + Sample_Locality);
    if ( fout ) {
        int Min =  Ref_str / 6 ;
        int Max =  Ref_str / 4 ;
        int count = 0 ;
        while (count < Numofmem_ref){
            int LocalitySize = get_RandonLocality(Min, Max);//用於曲每次locality大小 350 /6  < LocalitySize < 350 / 4
            int LocalityStart = get_RandonLocality(0, Ref_str - Max); // 0 ~ (350 - Max)
            int LocalityEnd = LocalityStart + LocalitySize ; // ( (0 ~ (350 - Max)) + LocalitySize )< 350
            for(int i = count ; i < count + LocalitySize ; i ++ ){
                int RandNum = get_RandonLocality(LocalityStart,LocalityEnd);
                Ref_str_LocalityArray[i] = RandNum;
                fout << RandNum;
                fout << "\t";
            }
            count += LocalitySize;
        }
        printf("success Sample_Locality.txt\n");
        fout.close();
    } else{
        printf("fail Sample_Locality\n");
    }
}

void RandonSample(const char *path) {
    string Sample_Randon = "Sample_Randon.txt";
    ofstream fout( path + Sample_Randon);
    if ( fout ) {
        for(int i = 0 ; i < Numofmem_ref;i++) {
            int RandNum = get_RandonNum(Ref_str);
            Ref_str_RandonArray[i] = RandNum;
            fout << RandNum;
            fout << "\t";
        }
        printf("success Sample_Randon.txt\n");
        fout.close();
    } else{
        printf("fail Sample_Randon.txt\n");
    }
}

int get_RandonLocality(int Min, int Max)
{
    double R = double(rand()) / double(RAND_MAX) ;
    int RandonNum  = (int)(R * (Max - Min));
    return  RandonNum + Min;
}

int get_RandonNum(int Num) {
    double R = double(rand()) / (RAND_MAX + 1.0);
    int RandonNum  = (int)(Num * R);
    return RandonNum;
}