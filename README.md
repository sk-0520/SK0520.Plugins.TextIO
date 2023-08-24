# SK0520.Plugins.TextIO

## ソースファイル

```javascript
/**
 * @name: なまえ!!
 * @update: x:\a.js
 * @parameters:パラメータ名1#display=なまえ1#require=true:boolean
 * @parameters:パラメータ名2#display=なまえ2#require=true:string
 * @parameters:パラメータ名3#require=true:integer
 * @parameters:パラメータ名4#require=true:decimal
 * @parameters:パラメータ名5#require=true:datetime
 * @debug-hot-reload: true
 */

function handler(args) {
    ...
    return "結果";
}
```
### ソースコメント

`:` で2分割される。

3分割される場合は `:` と `:` の間は、 `識別子#K1=V#K2=V` となる( `#` が区切りになる)

#### @name

スクリプトの名前

#### @update

アップデート先 URI

#### @parameters

パラメータ設定

##### 識別子

パラメータのプロパティ名

##### キーと値

| キー | 値 | デフォルト値 | 説明 |
|:-:|:-:|:-:|:--|
| `display` | `string` | **識別子** | パラメータ一覧表示文言 |
| `require` | `boolean` | **False** | 入力必須か |

##### 型

* `boolean`
* `string`
* `integer`
* `decimal`
* `datetime`

## エントリーポイント

関数 `handler` が実行時のエントリーポイントとなる。

### 引数

```javascript
args = {
    input: "入力テキスト",
    parameters: {
        "@parameters[識別子]": string|number
    }
}
```

### 戻り値

文字列を戻り値として出力に返す
